﻿using DeepL;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using ServerApiMikoAI.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace ServerApiMikoAI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly AuthorizationService _authorizationService;
        private static Translator translator = new Translator("kluczdoapitranslatora");

        public TranslationController(AuthorizationService authorization)
        {
            _authorizationService = authorization;
        }


        [HttpPost(Name = "Translate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(OperationId = "post")]
        public async Task<IActionResult> Post(TranslationMessage translationMessage)
        {
            string deviceId = HttpContext.Request.Headers["device_id"];
            string apiKey = HttpContext.Request.Headers["api_key"];

            var isAuthorized = await _authorizationService.IsDeviceAuthorized(deviceId, apiKey);

            if (!isAuthorized)
            {
                return Unauthorized("Invalid DeviceId or ApiKey.");
            }

            string result = await DeepLApi(translationMessage);

            switch (result)
            {
                case "-1":
                    return StatusCode(500, "Internal server error");
                default:
                    return Ok(result);
            }
        }

        public static async Task<string> DeepLApi([Required] TranslationMessage translationMessage)
        {
            try
            {
                var translatedMessage = await translator.TranslateTextAsync(
                    translationMessage.message,
                    translationMessage.translateFrom,
                    translationMessage.translateTo);

                return translatedMessage.Text;

            }
            catch (Exception ex)
            {
                return "-1";
            }

        }
    }
}
