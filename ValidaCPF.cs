using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class ValidateCpfFunction
{
    [FunctionName("ValidaCpf")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Processando...");

        string cpf = req.Query["cpf"];

        if (string.IsNullOrEmpty(cpf))
        {
            return new BadRequestObjectResult("Por favor digitar um CPF");
        }

        bool isValid = ValidaCPF(cpf);
        return new OkObjectResult(new { cpf, isValid });
    }

    private static bool ValidaCPF(string cpf)
    {
        cpf = Regex.Replace(cpf, "[^0-9]", "");

        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        int[] multipliers1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multipliers2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int sum = tempCpf.Select((t, i) => (t - '0') * multipliers1[i]).Sum();
        int remainder = sum % 11;
        int firstDigit = remainder < 2 ? 0 : 11 - remainder;

        tempCpf += firstDigit;
        sum = tempCpf.Select((t, i) => (t - '0') * multipliers2[i]).Sum();
        remainder = sum % 11;
        int secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return cpf.EndsWith(firstDigit.ToString() + secondDigit.ToString());
    }
}
