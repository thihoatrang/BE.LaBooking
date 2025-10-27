//using API.Gateway.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace API.Gateway.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class SagaController : ControllerBase
//    {
//        private readonly CrossServiceSagaService _sagaService;
//        private readonly ILogger<SagaController> _logger;

//        public SagaController(CrossServiceSagaService sagaService, ILogger<SagaController> logger)
//        {
//            _sagaService = sagaService;
//            _logger = logger;
//        }

//        [HttpPost("complete-user-registration")]
//        public async Task<IActionResult> StartCompleteUserRegistrationSaga([FromBody] object registrationData)
//        {
//            try
//            {
//                var result = await _sagaService.StartCompleteUserRegistrationSagaAsync(registrationData);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to start complete user registration saga");
//                return StatusCode(500, new { 
//                    Error = "Failed to start complete user registration saga", 
//                    Details = ex.Message 
//                });
//            }
//        }

//        [HttpPost("appointment-with-user-lawyer")]
//        public async Task<IActionResult> StartAppointmentWithUserLawyerSaga([FromBody] object appointmentData)
//        {
//            try
//            {
//                var result = await _sagaService.StartAppointmentWithUserLawyerSagaAsync(appointmentData);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to start appointment with user-lawyer saga");
//                return StatusCode(500, new { 
//                    Error = "Failed to start appointment with user-lawyer saga", 
//                    Details = ex.Message 
//                });
//            }
//        }

//        [HttpGet("state/{sagaId}")]
//        public IActionResult GetSagaState(string sagaId)
//        {
//            try
//            {
//                var state = _sagaService.GetSagaState(sagaId);
//                if (state == null)
//                {
//                    return NotFound(new { Message = "Saga not found" });
//                }
                
//                return Ok(state);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get saga state for {SagaId}", sagaId);
//                return StatusCode(500, new { 
//                    Error = "Failed to get saga state", 
//                    Details = ex.Message 
//                });
//            }
//        }

//        [HttpGet("all-states")]
//        public IActionResult GetAllSagaStates()
//        {
//            try
//            {
//                var states = _sagaService.GetAllSagaStates();
//                return Ok(states);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get all saga states");
//                return StatusCode(500, new { 
//                    Error = "Failed to get all saga states", 
//                    Details = ex.Message 
//                });
//            }
//        }
//    }
//}
