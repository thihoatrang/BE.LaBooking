"""
Script to run tests, parse results, and update Excel with actual test counts and status
"""
import subprocess
import re
import csv
import json
from pathlib import Path
from datetime import datetime
from collections import defaultdict
import xml.etree.ElementTree as ET

BASE_PATH = Path(__file__).parent

def run_tests():
    """Run all tests and capture output"""
    print("Running tests...")
    try:
        result = subprocess.run(
            ["dotnet", "test", "--logger", "trx;LogFileName=test-results.trx", "--no-build"],
            capture_output=True,
            text=True,
            cwd=BASE_PATH
        )
        return result.stdout, result.stderr
    except Exception as e:
        print(f"Error running tests: {e}")
        return "", str(e)

def parse_trx_files():
    """Parse TRX files to get test results per test class"""
    test_results = defaultdict(lambda: {"total": 0, "passed": 0, "failed": 0, "skipped": 0})
    
    # Find all TRX files
    trx_files = list(BASE_PATH.rglob("**/TestResults/*.trx"))
    
    for trx_file in trx_files:
        try:
            tree = ET.parse(trx_file)
            root = tree.getroot()
            
            # Namespace handling
            namespaces = {'ns': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
            
            # Get all test results
            for unit_test_result in root.findall('.//ns:UnitTestResult', namespaces):
                test_name = unit_test_result.get('testName', '')
                outcome = unit_test_result.get('outcome', '')
                
                # Extract test class from test name (format: ClassName.MethodName)
                if '.' in test_name:
                    class_name = test_name.rsplit('.', 1)[0]
                    class_name = class_name.split('+')[0]  # Handle nested classes
                    
                    test_results[class_name]["total"] += 1
                    if outcome == "Passed":
                        test_results[class_name]["passed"] += 1
                    elif outcome == "Failed":
                        test_results[class_name]["failed"] += 1
                    elif outcome == "Skipped":
                        test_results[class_name]["skipped"] += 1
        except Exception as e:
            print(f"Error parsing {trx_file}: {e}")
            continue
    
    return test_results

def extract_test_class_name(full_test_class_name):
    """Extract test class name from full namespace path"""
    # Full name format: Namespace.ClassName or Namespace.Tests.ClassName
    # We want just the ClassName part
    if '.' in full_test_class_name:
        parts = full_test_class_name.split('.')
        # Get the last part which should be the class name
        class_name = parts[-1]
        return class_name
    return full_test_class_name

def map_test_class_to_component(test_class_name):
    """Map test class name to component name from CSV"""
    # Test class names are typically: ComponentNameTests
    # Component names in CSV are: ComponentName
    class_name = extract_test_class_name(test_class_name)
    component_name = class_name.replace("Tests", "")
    return component_name

def update_csv_with_results(test_results):
    """Update CSV files with actual test results"""
    detailed_csv = BASE_PATH / "Unit_Test_Statistics_Detailed.csv"
    
    if not detailed_csv.exists():
        print(f"CSV file not found: {detailed_csv}")
        return
    
    # Read current CSV
    rows = []
    with open(detailed_csv, 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f)
        fieldnames = reader.fieldnames
        for row in reader:
            rows.append(row)
    
    # Create a mapping from component name to test results
    component_to_results = {}
    for test_class, results in test_results.items():
        component_name = map_test_class_to_component(test_class)
        component_to_results[component_name] = results
    
    # Update rows with test results
    updated_rows = []
    for row in rows:
        component_name = row["Component Name"]
        test_file = row["Test File"]
        test_class_name = test_file.replace(".cs", "")
        
        # Try to find matching test class by component name
        matched = False
        matched_results = None
        
        if component_name in component_to_results:
            matched = True
            matched_results = component_to_results[component_name]
        
        if matched:
            total = matched_results["total"]
            passed = matched_results["passed"]
            failed = matched_results["failed"]
            
            # Determine status
            if total == 0:
                status = "Pending"
            elif failed > 0:
                status = f"Failed ({failed})"
            elif passed == total:
                status = "Passed"
            else:
                status = "Partial"
            
            row["Status"] = status
            row["Test Count"] = str(total)
        else:
            # If no tests found, check if test file exists
            test_file_path = None
            module = row["Module"]
            comp_type = row["Type"]
            
            # Build test file path
            if comp_type == "Service" or comp_type == "SagaService":
                if "Saga" in row["Namespace"]:
                    test_file_path = BASE_PATH / f"{module}.Application.Tests" / "Services" / "Saga" / test_file
                else:
                    test_file_path = BASE_PATH / f"{module}.Application.Tests" / "Services" / test_file
            elif comp_type == "Controller":
                if module == "Gateway":
                    test_file_path = BASE_PATH / "API.Gateway.Tests" / "Controllers" / test_file
                elif module == "Lawyers":
                    test_file_path = BASE_PATH / "Lawyers.Services.API.Tests" / "Controllers" / test_file
                else:
                    test_file_path = BASE_PATH / f"{module}.Services.API.Tests" / "Controllers" / test_file
            elif comp_type == "Repository":
                test_file_path = BASE_PATH / f"{module}.Infrastructure.Tests" / "Repository" / test_file
            
            if test_file_path and test_file_path.exists():
                # File exists but no tests found - might be empty or not run
                row["Status"] = "Pending"
                row["Test Count"] = "0"
            else:
                # File doesn't exist
                row["Status"] = "Not Created"
                row["Test Count"] = "0"
        
        updated_rows.append(row)
    
    # Write updated CSV
    with open(detailed_csv, 'w', encoding='utf-8-sig', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(updated_rows)
    
    print(f"\nUpdated {detailed_csv} with test results")
    
    # Update summary CSV
    update_summary_csv(updated_rows)

def update_summary_csv(detailed_rows):
    """Update summary CSV based on detailed rows"""
    summary_csv = BASE_PATH / "Unit_Test_Statistics_Summary.csv"
    
    # Count by category
    counts = {
        "Services": {"count": 0, "tests": 0, "passed": 0},
        "Controllers": {"count": 0, "tests": 0, "passed": 0},
        "Repositories": {"count": 0, "tests": 0, "passed": 0}
    }
    
    for row in detailed_rows:
        comp_type = row["Type"]
        if comp_type in ["Service", "SagaService"]:
            category = "Services"
        elif comp_type == "Controller":
            category = "Controllers"
        elif comp_type == "Repository":
            category = "Repositories"
        else:
            continue
        
        counts[category]["count"] += 1
        test_count = int(row.get("Test Count", 0))
        counts[category]["tests"] += test_count
        
        # Check if passed
        status = row.get("Status", "Pending")
        if "Passed" in status or status == "Passed":
            counts[category]["passed"] += 1
    
    # Calculate totals
    total_count = sum(c["count"] for c in counts.values())
    total_tests = sum(c["tests"] for c in counts.values())
    total_passed = sum(c["passed"] for c in counts.values())
    
    # Write summary CSV
    with open(summary_csv, 'w', encoding='utf-8-sig', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(["Category", "Count", "Tests Created", "Tests Passed", "Coverage %"])
        
        for category, data in counts.items():
            coverage = f"{(data['tests'] / data['count'] * 100):.1f}%" if data['count'] > 0 else "0%"
            writer.writerow([
                category,
                data["count"],
                data["tests"],
                data["passed"],
                coverage
            ])
        
        total_coverage = f"{(total_tests / total_count * 100):.1f}%" if total_count > 0 else "0%"
        writer.writerow([
            "TOTAL",
            total_count,
            total_tests,
            total_passed,
            total_coverage
        ])
    
    print(f"Updated {summary_csv}")

def main():
    """Main function"""
    print("=" * 60)
    print("Running Tests and Updating Excel Statistics")
    print("=" * 60)
    
    # Step 1: Run tests
    print("\n[1/3] Running tests...")
    stdout, stderr = run_tests()
    
    if stderr:
        print(f"Test execution warnings/errors: {stderr[:500]}")
    
    # Step 2: Parse test results
    print("\n[2/3] Parsing test results...")
    test_results = parse_trx_files()
    
    if not test_results:
        print("No test results found. Make sure tests were executed successfully.")
        return
    
    print(f"Found test results for {len(test_results)} test classes:")
    for test_class, results in sorted(test_results.items()):
        print(f"  - {test_class}: {results['total']} tests ({results['passed']} passed, {results['failed']} failed)")
    
    # Step 3: Update CSV files
    print("\n[3/3] Updating CSV files with test results...")
    update_csv_with_results(test_results)
    
    # Step 4: Regenerate Excel
    print("\n[4/4] Regenerating Excel file...")
    try:
        from create_excel_statistics import create_excel_statistics
        excel_file = create_excel_statistics()
        print(f"\nSuccess! Excel file updated: {excel_file}")
    except Exception as e:
        print(f"Error regenerating Excel: {e}")
        print("You can manually run: python create_excel_statistics.py")
    
    print("\n" + "=" * 60)
    print("Completed!")
    print("=" * 60)

if __name__ == "__main__":
    main()

