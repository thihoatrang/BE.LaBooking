using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chat.Application.Services
{
    public class LegalDocumentService : ILegalDocumentService
    {
        private readonly IVectorStoreService _vectorStore;
        private readonly IEmbeddingService _embeddingService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LegalDocumentService(IVectorStoreService vectorStore, IEmbeddingService embeddingService, HttpClient httpClient, IConfiguration configuration)
        {
            _vectorStore = vectorStore;
            _embeddingService = embeddingService;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task IndexLegalDocumentsAsync(CancellationToken cancellationToken = default)
        {
            // 1. Crawl data từ các nguồn
            var legalDocuments = await CrawlLegalDocumentsAsync(cancellationToken);

            // 2. Chuẩn hóa + phân mảnh và lập chỉ mục
            foreach (var doc in legalDocuments)
            {
                var normalized = NormalizeWhitespace(doc.Content);
                var chunks = ChunkText(normalized, 1500, 200); // tokens-approx chars

                for (int i = 0; i < chunks.Count; i++)
                {
                    var metadata = new Dictionary<string, object>
                    {
                        ["type"] = "legal_document",
                        ["source"] = doc.Source,
                        ["document_type"] = doc.DocumentType,
                        ["issued_date"] = doc.IssuedDate,
                        ["effective_date"] = doc.EffectiveDate,
                        ["issuing_authority"] = doc.IssuingAuthority,
                        ["chunk_index"] = i
                    };

                    await _vectorStore.IndexDocumentAsync($"law_{doc.Id}_chunk_{i}", chunks[i], metadata, cancellationToken);
                }
            }
        }

        public async Task<string> SearchLegalDocumentsAsync(string query, int topK = 3, CancellationToken cancellationToken = default)
        {
            var results = await _vectorStore.SearchSimilarAsync(query, topK, cancellationToken);
            var legalResults = results.Where(r =>
                r.Metadata.ContainsKey("type") && r.Metadata["type"].ToString() == "legal_document");

            if (!legalResults.Any())
                return string.Empty;

            var context = "CÁC VĂN BẢN PHÁP LUẬT LIÊN QUAN:\n";
            foreach (var result in legalResults)
            {
                context += $"\n{result.Content}\n(Nguồn: {result.Metadata["source"]})\n";
            }

            return context;
        }

        private async Task<List<LegalDocument>> CrawlLegalDocumentsAsync(CancellationToken cancellationToken)
        {
            var documents = new List<LegalDocument>();

            // 1) API trả phí: Thư viện Pháp luật (nếu có key cấu hình)
            var tvplApiKey = _configuration["LegalSources:ThuVienPhapLuat:ApiKey"];
            if (!string.IsNullOrWhiteSpace(tvplApiKey))
            {
                try { documents.AddRange(await FetchFromThuVienPhapLuatAsync(tvplApiKey, cancellationToken)); }
                catch { /* log if needed */ }
            }

            // 2) Nguồn công khai: Công báo Chính phủ
            try { documents.AddRange(await FetchFromCongBaoAsync(cancellationToken)); } catch { /* log */ }

            // 3) VBPL (Bộ Tư pháp)
            try { documents.AddRange(await FetchFromVbplAsync(cancellationToken)); } catch { /* log */ }

            // 4) MOJ (Bộ Tư pháp)
            try { documents.AddRange(await FetchFromMojAsync(cancellationToken)); } catch { /* log */ }

            // 5) Fallback dữ liệu mẫu nếu chưa có gì
            if (documents.Count == 0)
            {
                documents.AddRange(await GetSampleLegalDocumentsAsync());
            }

            return documents;
        }

        private async Task<List<LegalDocument>> GetSampleLegalDocumentsAsync()
        {
            // Dữ liệu mẫu - thay thế bằng crawler thực tế
            return new List<LegalDocument>
            {
                 new LegalDocument
        {
            Id = "bl_2015_99",
            Content = @"BỘ LUẬT DÂN SỰ 2015
Số: 99/2015/QH13 - Ngày ban hành: 24/11/2015 - Có hiệu lực: 01/01/2017

Điều 1. Phạm vi điều chỉnh
Bộ luật này quy định địa vị pháp lý, chuẩn mực pháp lý về cách ứng xử của cá nhân, pháp nhân; quyền, nghĩa vụ về nhân thân và tài sản của cá nhân, pháp nhân trong quan hệ được hình thành trên cơ sở bình đẳng, tự do ý chí, độc lập về tài sản và tự chịu trách nhiệm.

Điều 2. Giao dịch dân sự
Giao dịch dân sự là hợp đồng hoặc hành vi pháp lý đơn phương làm phát sinh, thay đổi hoặc chấm dứt quyền, nghĩa vụ dân sự.

Điều 117. Điều kiện có hiệu lực của giao dịch dân sự
1. Chủ thể có năng lực pháp luật dân sự, năng lực hành vi dân sự phù hợp với giao dịch dân sự được xác lập.
2. Chủ thể tham gia giao dịch dân sự hoàn toàn tự nguyện.
3. Mục đích và nội dung của giao dịch dân sự không vi phạm điều cấm của luật, không trái đạo đức xã hội.
4. Hình thức của giao dịch dân sự là điều kiện có hiệu lực của giao dịch trong trường hợp luật có quy định.",
            Source = "Bộ luật Dân sự 2015",
            DocumentType = "Bộ luật",
            IssuedDate = "2015-11-24",
            EffectiveDate = "2017-01-01",
            IssuingAuthority = "Quốc hội"
        },
        new LegalDocument
        {
            Id = "bl_2015_100",
            Content = @"BỘ LUẬT HÌNH SỰ 2015
Số: 100/2015/QH13 - Ngày ban hành: 27/11/2015 - Có hiệu lực: 01/01/2018

Điều 1. Nhiệm vụ của Bộ luật Hình sự
Bộ luật Hình sự có nhiệm vụ bảo vệ chủ quyền quốc gia, an ninh của đất nước, bảo vệ chế độ xã hội chủ nghĩa, quyền con người, quyền công dân, bảo vệ quyền bình đẳng giữa các dân tộc, bảo vệ lợi ích của Nhà nước, quyền và lợi ích hợp pháp của tổ chức, cá nhân, bảo vệ trật tự pháp luật, chống mọi hành vi phạm tội.

Điều 8. Tội phạm
Tội phạm là hành vi nguy hiểm cho xã hội được quy định trong Bộ luật Hình sự, do người có năng lực trách nhiệm hình sự thực hiện một cách cố ý hoặc vô ý.

Điều 174. Tội tham ô tài sản
1. Người nào lợi dụng chức vụ, quyền hạn chiếm đoạt tài sản mà mình có trách nhiệm quản lý có giá trị từ 2.000.000 đồng đến dưới 100.000.000 đồng hoặc dưới 2.000.000 đồng nhưng đã bị xử lý kỷ luật về hành vi này mà còn vi phạm, thì bị phạt cải tạo không giam giữ đến 03 năm hoặc phạt tù từ 01 năm đến 05 năm.",
            Source = "Bộ luật Hình sự 2015",
            DocumentType = "Bộ luật",
            IssuedDate = "2015-11-27",
            EffectiveDate = "2018-01-01",
            IssuingAuthority = "Quốc hội"
        },
        new LegalDocument
        {
            Id = "nd_2019_01",
            Content = @"NGHỊ ĐỊNH VỀ XỬ PHẠT VI PHẠM HÀNH CHÍNH
Số: 01/2019/NĐ-CP - Ngày ban hành: 01/01/2019 - Có hiệu lực: 01/02/2019

Điều 1. Phạm vi điều chỉnh
Nghị định này quy định về hành vi vi phạm hành chính, hình thức xử phạt, mức phạt, biện pháp khắc phục hậu quả đối với hành vi vi phạm hành chính trong các lĩnh vực quản lý nhà nước.

Điều 5. Mức phạt tiền
1. Mức phạt tiền tối đa trong lĩnh vực giao thông đường bộ là 40.000.000 đồng đối với cá nhân và 80.000.000 đồng đối với tổ chức.
2. Mức phạt tiền tối đa trong lĩnh vực xây dựng là 60.000.000 đồng đối với cá nhân và 120.000.000 đồng đối với tổ chức.

Điều 15. Vi phạm quy định về tốc độ
1. Phạt tiền từ 800.000 đồng đến 1.200.000 đồng đối với người điều khiển xe chạy quá tốc độ quy định từ 05 km/h đến dưới 10 km/h.",
            Source = "Nghị định 01/2019/NĐ-CP",
            DocumentType = "Nghị định",
            IssuedDate = "2019-01-01",
            EffectiveDate = "2019-02-01",
            IssuingAuthority = "Chính phủ"
        }
    };
        }

        private static string NormalizeWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var withoutHtml = Regex.Replace(input, "<[^>]+>", " ", RegexOptions.Singleline);
            var condensed = Regex.Replace(withoutHtml, "\r?\n\\s*\r?\n", "\n");
            var normalizedSpaces = Regex.Replace(condensed, "[\\t ]+", " ");
            return normalizedSpaces.Trim();
        }

        private static List<string> ChunkText(string text, int chunkSize, int overlap)
        {
            var chunks = new List<string>();
            if (string.IsNullOrEmpty(text)) return chunks;
            var length = text.Length;
            var start = 0;
            while (start < length)
            {
                var end = Math.Min(start + chunkSize, length);
                var slice = text.Substring(start, end - start);
                chunks.Add(slice);
                if (end == length) break;
                start = Math.Max(end - overlap, start + 1);
            }
            return chunks;
        }

        // =============== Fetchers (simplified placeholders) ===============
        private async Task<List<LegalDocument>> FetchFromThuVienPhapLuatAsync(string apiKey, CancellationToken cancellationToken)
        {
            // Placeholder: depends on paid API contract. Keep structure for future.
            // Example pseudo-call using configured endpoint
            var baseUrl = _configuration["LegalSources:ThuVienPhapLuat:BaseUrl"] ?? "";
            if (string.IsNullOrWhiteSpace(baseUrl)) return new List<LegalDocument>();
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/documents?limit=20");
            req.Headers.Add("Authorization", $"Bearer {apiKey}");
            var res = await _httpClient.SendAsync(req, cancellationToken);
            if (!res.IsSuccessStatusCode) return new List<LegalDocument>();
            var content = await res.Content.ReadAsStringAsync(cancellationToken);
            // TODO: parse real schema
            return new List<LegalDocument>();
        }

        private async Task<List<LegalDocument>> FetchFromCongBaoAsync(CancellationToken cancellationToken)
        {
            // Basic fetch from a public listing or RSS if available (placeholder)
            var results = new List<LegalDocument>();
            var url = _configuration["LegalSources:CongBao:RssUrl"] ?? "";
            if (string.IsNullOrWhiteSpace(url)) return results;
            var xml = await _httpClient.GetStringAsync(url, cancellationToken);
            // TODO: parse RSS to extract title/link/date and fetch detail pages
            return results;
        }

        private async Task<List<LegalDocument>> FetchFromVbplAsync(CancellationToken cancellationToken)
        {
            var results = new List<LegalDocument>();
            var listUrl = _configuration["LegalSources:Vbpl:ListUrl"] ?? "";
            if (string.IsNullOrWhiteSpace(listUrl)) return results;
            var html = await _httpClient.GetStringAsync(listUrl, cancellationToken);
            // TODO: parse HTML list and detail pages respecting robots/terms
            return results;
        }

        //private async Task<List<LegalDocument>> FetchFromMojAsync(CancellationToken cancellationToken)
        //{
        //    var results = new List<LegalDocument>();
        //    var listUrl = _configuration["LegalSources:Moj:ListUrl"] ?? "";
        //    if (string.IsNullOrWhiteSpace(listUrl)) return results;
        //    var html = await _httpClient.GetStringAsync(listUrl, cancellationToken);
        //    // TODO: parse HTML list and detail pages
        //    return results;
        //}
        private async Task<List<LegalDocument>> FetchFromMojAsync(CancellationToken cancellationToken)
        {
            var documents = new List<LegalDocument>();
            try
            {
                // Cho phép dùng dữ liệu mẫu qua cấu hình để test nhanh
                var useSample = (_configuration["LegalSources:Moj:UseSample"] ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                if (useSample)
                {
                    Console.WriteLine("MOJ sample mode enabled via configuration. Using sample documents.");
                    return GetSampleMojDocuments();
                }

                var listUrl = _configuration["LegalSources:Moj:ListUrl"] ?? "https://moj.gov.vn/van-ban-phap-luat/Pages/default.aspx";
                var baseUrl = _configuration["LegalSources:Moj:BaseUrl"] ?? "https://moj.gov.vn";

                Console.WriteLine($"Starting MOJ crawl from: {listUrl}");

                // 1. Lấy HTML trang danh sách
                var html = await _httpClient.GetStringAsync(listUrl, cancellationToken);

                // 2. Parse HTML để lấy links đến văn bản chi tiết
                var docLinks = ParseMojDocumentLinks(html, baseUrl);
                Console.WriteLine($"Found {docLinks.Count} document links");

                // 3. Duyệt qua từng link để lấy nội dung chi tiết
                foreach (var link in docLinks.Take(5)) // Giới hạn 5 văn bản để test
                {
                    try
                    {
                        Console.WriteLine($"Fetching document: {link}");
                        var doc = await FetchMojDocumentDetailAsync(link, cancellationToken);
                        if (doc != null)
                        {
                            documents.Add(doc);
                            Console.WriteLine($"Successfully fetched: {doc.Id} - {doc.Source}");

                            // Delay để tránh bị block
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching MOJ doc {link}: {ex.Message}");
                    }

                    if (cancellationToken.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FetchFromMojAsync: {ex.Message}");
            }

            if (documents.Count == 0)
            {
                Console.WriteLine("MOJ crawl yielded 0 documents. Falling back to sample documents for testing.");
                documents = GetSampleMojDocuments();
            }

            Console.WriteLine($"MOJ crawl completed. Found {documents.Count} documents");
            return documents;
        }

        private List<string> ParseMojDocumentLinks(string html, string baseUrl)
        {
            var links = new List<string>();
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Tìm tất cả các link đến văn bản pháp luật
                // Selector này có thể cần điều chỉnh tùy theo cấu trúc HTML thực tế
                var linkNodes = htmlDoc.DocumentNode.SelectNodes(
                    "//a[contains(@href, 'van-ban-phap-luat')] | " +
                    "//a[contains(@class, 'document')] | " +
                    "//a[contains(text(), 'văn bản')] | " +
                    "//div[contains(@class, 'news-item')]//a"
                );

                if (linkNodes != null)
                {
                    foreach (var node in linkNodes)
                    {
                        var href = node.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            // Chuyển đổi relative URL thành absolute URL
                            var fullUrl = href.StartsWith("http") ? href :
                                         href.StartsWith("/") ? baseUrl + href :
                                         baseUrl + "/" + href;

                            // Lọc chỉ lấy link có chứa từ khóa liên quan đến văn bản pháp luật
                            if (IsLegalDocumentLink(fullUrl) && !links.Contains(fullUrl))
                            {
                                links.Add(fullUrl);
                            }
                        }
                    }
                }

                // Fallback: nếu không tìm thấy bằng selector, thử tìm bằng từ khóa
                if (links.Count == 0)
                {
                    var allLinks = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
                    if (allLinks != null)
                    {
                        foreach (var node in allLinks)
                        {
                            var href = node.GetAttributeValue("href", "");
                            var text = node.InnerText.ToLower();

                            if (!string.IsNullOrEmpty(href) &&
                                (text.Contains("nghị định") || text.Contains("thông tư") ||
                                 text.Contains("quyết định") || text.Contains("chỉ thị") ||
                                 text.Contains("luật") || text.Contains("bộ luật")))
                            {
                                var fullUrl = href.StartsWith("http") ? href :
                                             href.StartsWith("/") ? baseUrl + href :
                                             baseUrl + "/" + href;

                                if (!links.Contains(fullUrl))
                                {
                                    links.Add(fullUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ParseMojDocumentLinks: {ex.Message}");
            }

            return links.Distinct().ToList();
        }

        private bool IsLegalDocumentLink(string url)
        {
            var lowerUrl = url.ToLower();
            var legalKeywords = new[]
            {
        "nghị định", "thông tư", "quyết định", "chỉ thị",
        "luật", "bộ luật", "van-ban", "document", "phap-luat",
        "qd", "tt", "nd", "ct"
    };

            return legalKeywords.Any(keyword => lowerUrl.Contains(keyword));
        }

        private async Task<LegalDocument> FetchMojDocumentDetailAsync(string docUrl, CancellationToken cancellationToken)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(docUrl, cancellationToken);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Extract thông tin từ trang chi tiết
                var document = new LegalDocument
                {
                    Id = GenerateDocumentId(docUrl),
                    Source = "Bộ Tư pháp (MOJ)",
                    Content = ExtractContent(htmlDoc),
                    DocumentType = ExtractDocumentType(htmlDoc),
                    IssuingAuthority = ExtractIssuingAuthority(htmlDoc),
                    IssuedDate = ExtractIssuedDate(htmlDoc),
                    EffectiveDate = ExtractEffectiveDate(htmlDoc)
                };

                // Nếu không extract được nội dung, có thể dùng fallback
                if (string.IsNullOrEmpty(document.Content))
                {
                    document.Content = ExtractFallbackContent(htmlDoc);
                }

                return string.IsNullOrEmpty(document.Content) ? null : document;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FetchMojDocumentDetailAsync for {docUrl}: {ex.Message}");
                return null;
            }
        }

        private string GenerateDocumentId(string url)
        {
            // Tạo ID duy nhất từ URL
            var uri = new Uri(url);
            var segments = uri.Segments.Where(s => s != "/").ToArray();
            return "moj_" + string.Join("_", segments).Replace("/", "_").Replace(".aspx", "");
        }

        private string ExtractContent(HtmlDocument htmlDoc)
        {
            // Ưu tiên các vùng chứa nội dung chính
            var contentSelectors = new[]
            {
        "//div[contains(@class, 'content')]",
        "//div[contains(@class, 'document-content')]",
        "//div[contains(@class, 'main-content')]",
        "//article",
        "//div[contains(@class, 'news-content')]",
        "//div[contains(@id, 'content')]"
    };

            foreach (var selector in contentSelectors)
            {
                var contentNode = htmlDoc.DocumentNode.SelectSingleNode(selector);
                if (contentNode != null)
                {
                    var text = contentNode.InnerText;
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 100)
                    {
                        return NormalizeWhitespace(text);
                    }
                }
            }

            return string.Empty;
        }

        private string ExtractFallbackContent(HtmlDocument htmlDoc)
        {
            // Fallback: lấy tất cả text có ý nghĩa
            var body = htmlDoc.DocumentNode.SelectSingleNode("//body");
            if (body != null)
            {
                // Loại bỏ script và style
                foreach (var node in body.SelectNodes("//script | //style | //nav | //header | //footer") ?? Enumerable.Empty<HtmlNode>())
                {
                    node.Remove();
                }

                var text = body.InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return NormalizeWhitespace(text);
                }
            }

            return string.Empty;
        }

        private string ExtractDocumentType(HtmlDocument htmlDoc)
        {
            var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "";
            var heading = htmlDoc.DocumentNode.SelectSingleNode("//h1 | //h2")?.InnerText ?? "";

            var fullText = (title + " " + heading).ToLower();

            if (fullText.Contains("nghị định")) return "Nghị định";
            if (fullText.Contains("thông tư")) return "Thông tư";
            if (fullText.Contains("quyết định")) return "Quyết định";
            if (fullText.Contains("chỉ thị")) return "Chỉ thị";
            if (fullText.Contains("luật")) return "Luật";
            if (fullText.Contains("bộ luật")) return "Bộ luật";

            return "Văn bản pháp luật";
        }

        private string ExtractIssuingAuthority(HtmlDocument htmlDoc)
        {
            // Tìm cơ quan ban hành trong nội dung
            var content = htmlDoc.DocumentNode.InnerText;

            var authorities = new[]
            {
        "Chính phủ", "Quốc hội", "Bộ Tư pháp", "Bộ trưởng",
        "Thủ tướng", "Chủ tịch nước", "Ủy ban thường vụ Quốc hội"
    };

            foreach (var authority in authorities)
            {
                if (content.Contains(authority))
                    return authority;
            }

            return "Bộ Tư pháp";
        }

        private string ExtractIssuedDate(HtmlDocument htmlDoc)
        {
            // Tìm ngày ban hành trong nội dung
            var content = htmlDoc.DocumentNode.InnerText;

            // Regex để tìm ngày tháng (dd/mm/yyyy hoặc dd-mm-yyyy)
            var datePattern = @"\b(\d{1,2})[/-](\d{1,2})[/-](\d{4})\b";
            var match = System.Text.RegularExpressions.Regex.Match(content, datePattern);

            if (match.Success)
            {
                try
                {
                    var day = int.Parse(match.Groups[1].Value);
                    var month = int.Parse(match.Groups[2].Value);
                    var year = int.Parse(match.Groups[3].Value);

                    if (year > 1900 && year <= DateTime.Now.Year)
                    {
                        return $"{year:0000}-{month:00}-{day:00}";
                    }
                }
                catch
                {
                    // Ignore parse errors
                }
            }

            return DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd"); // Fallback: 1 năm trước
        }

        private string ExtractEffectiveDate(HtmlDocument htmlDoc)
        {
            // Thường ngày hiệu lực = ngày ban hành + 45 ngày
            var issuedDate = ExtractIssuedDate(htmlDoc);
            if (DateTime.TryParse(issuedDate, out var date))
            {
                return date.AddDays(45).ToString("yyyy-MM-dd");
            }

            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        private List<LegalDocument> GetSampleMojDocuments()
        {
            // Bộ dữ liệu mẫu mô phỏng văn bản từ MOJ để test indexing/search
            return new List<LegalDocument>
            {
                new LegalDocument
                {
                    Id = "moj_qd_23_2024_btpthcn",
                    Source = "Bộ Tư pháp (MOJ)",
                    DocumentType = "Quyết định",
                    IssuingAuthority = "Bộ Tư pháp",
                    IssuedDate = "2024-06-15",
                    EffectiveDate = "2024-07-30",
                    Content = @"QUYẾT ĐỊNH 23/2024/QĐ-BTP
Về việc ban hành Quy chế phối hợp trong công tác trợ giúp pháp lý

Điều 1. Phạm vi điều chỉnh
Quy chế này quy định nguyên tắc, nội dung, trách nhiệm phối hợp giữa các cơ quan, tổ chức trong công tác trợ giúp pháp lý cho người được trợ giúp pháp lý theo quy định của pháp luật.

Điều 2. Đối tượng áp dụng
1. Trung tâm trợ giúp pháp lý Nhà nước.
2. Tổ chức hành nghề luật sư, luật sư ký hợp đồng thực hiện trợ giúp pháp lý.
3. Cơ quan tiến hành tố tụng và các cơ quan, tổ chức có liên quan.

Điều 3. Hướng dẫn thực hiện trợ giúp pháp lý
1. Trợ giúp pháp lý được thực hiện thông qua các hình thức: tư vấn pháp luật, tham gia tố tụng, đại diện ngoài tố tụng, hỗ trợ pháp lý khác.
2. Trung tâm trợ giúp pháp lý Nhà nước có trách nhiệm tiếp nhận, phân loại và phân công luật sư, tư vấn viên pháp luật thực hiện trợ giúp pháp lý.
3. Luật sư, tư vấn viên pháp luật thực hiện trợ giúp pháp lý phải tuân thủ quy định về đạo đức nghề nghiệp và quy trình nghiệp vụ.

Điều 4. Quy trình phối hợp
1. Khi nhận được yêu cầu trợ giúp pháp lý, cơ quan tiếp nhận phải ghi nhận đầy đủ thông tin về đối tượng được trợ giúp, nội dung vụ việc.
2. Cơ quan tiếp nhận chuyển giao yêu cầu cho Trung tâm trợ giúp pháp lý trong thời hạn 03 ngày làm việc.
3. Trung tâm trợ giúp pháp lý có trách nhiệm phân công người thực hiện trợ giúp pháp lý trong thời hạn 05 ngày làm việc.

Điều 5. Hướng dẫn về phạm vi trợ giúp pháp lý
1. Trợ giúp pháp lý được thực hiện cho các đối tượng: người có công với cách mạng, người thuộc hộ nghèo, trẻ em, người từ đủ 60 tuổi trở lên sống cô đơn không nơi nương tựa.
2. Các lĩnh vực được trợ giúp pháp lý: dân sự, hôn nhân và gia đình, lao động, đất đai, hình sự, hành chính.
3. Trợ giúp pháp lý được miễn phí theo quy định của pháp luật về trợ giúp pháp lý."
                },
                new LegalDocument
                {
                    Id = "moj_tt_08_2023_btpthcn",
                    Source = "Bộ Tư pháp (MOJ)",
                    DocumentType = "Thông tư",
                    IssuingAuthority = "Bộ Tư pháp",
                    IssuedDate = "2023-11-10",
                    EffectiveDate = "2024-01-01",
                    Content = @"THÔNG TƯ 08/2023/TT-BTP
Hướng dẫn về đăng ký và quản lý hộ tịch trên môi trường điện tử

Điều 3. Đăng ký khai sinh trực tuyến
1. Hồ sơ đăng ký khai sinh được nộp qua Cổng dịch vụ công quốc gia hoặc Hệ thống thông tin đăng ký và quản lý hộ tịch.
2. Cơ quan đăng ký hộ tịch tiếp nhận, kiểm tra và giải quyết hồ sơ theo quy định."
                },
                new LegalDocument
                {
                    Id = "moj_nd_12_2022_cp",
                    Source = "Bộ Tư pháp (MOJ)",
                    DocumentType = "Nghị định",
                    IssuingAuthority = "Chính phủ",
                    IssuedDate = "2022-12-20",
                    EffectiveDate = "2023-02-05",
                    Content = @"NGHỊ ĐỊNH 12/2022/NĐ-CP
Quy định về xử phạt vi phạm hành chính trong lĩnh vực bổ trợ tư pháp

Điều 5. Mức phạt tiền
1. Phạt tiền từ 5.000.000 đồng đến 10.000.000 đồng đối với hành vi không niêm yết phí dịch vụ công chứng theo quy định.
2. Biện pháp khắc phục hậu quả: buộc niêm yết đầy đủ phí, lệ phí theo quy định."
                }
            };
        }
    }

    public class LegalDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string IssuedDate { get; set; } = string.Empty;
        public string EffectiveDate { get; set; } = string.Empty;
        public string IssuingAuthority { get; set; } = string.Empty;
    }
}