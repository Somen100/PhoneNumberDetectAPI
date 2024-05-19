using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class PhoneNumberController : ControllerBase
    {
        private static readonly Regex PhoneNumberRegex = new Regex(@"\(?\b\d{3}[-.)]?\s?\d{3}[-.]?\d{4}\b", RegexOptions.Compiled);

        [HttpPost("detect-from-text")]
        public IActionResult DetectFromText([FromBody] InputTextModel input)
        {
            if (string.IsNullOrEmpty(input.Text))
            {
                return BadRequest("Input text cannot be empty.");
            }

            var phoneNumbers = DetectPhoneNumbers(input.Text);
            return Ok(phoneNumbers);
        }

        //private List<string> DetectPhoneNumbers(string input)
        //{
        //    var matches = PhoneNumberRegex.Matches(input);
        //    return matches.Select(match => match.Value).ToList();
        //}

        [HttpPost("detect-from-file")]
        public async Task<IActionResult> DetectFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File cannot be empty.");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var content = await reader.ReadToEndAsync();
                var phoneNumbers = DetectPhoneNumbers(content);
                return Ok(phoneNumbers);
            }
        }

        //

        private List<PhoneNumberInfo> DetectPhoneNumbers(string inputText)
        {
            List<PhoneNumberInfo> phoneNumbers = new List<PhoneNumberInfo>();

          
            string pattern = @"\+?\(?\d{1,3}\)?[-.\s]?\(?\d{2,3}\)?[-.\s]?\d{3}[-.\s]?\d{4}";

            // Match phone numbers in the input text using the regular expression
            MatchCollection matches = Regex.Matches(inputText, pattern);

            // Add matched phone numbers to the list
            foreach (Match match in matches)
            {
                phoneNumbers.Add(new PhoneNumberInfo
                {
                    Number = match.Value,
                    Format = GetPhoneNumberFormat(match.Value)
                });
            }

            return phoneNumbers;
        }

        private string GetPhoneNumberFormat(string phoneNumber)
        {
            // Determine the format of the phone number based on its pattern
            if (Regex.IsMatch(phoneNumber, @"^\d{10}$"))
            {
                return "10-digit";
            }
            else if ((Regex.IsMatch(phoneNumber, @"^\+\d{1,3}-\d{10}$"))||((Regex.IsMatch(phoneNumber, @"^\d{3}-\d{3}-\d{4}$")))
                ||(Regex.IsMatch(phoneNumber, @"^[०-९]{3}-[०-९]{3}-[०-९]{4}$")) || (Regex.IsMatch(phoneNumber, @"^[०-९]{3}-[०-९]{3}-[०-९]{4}$")))   //^\+९१-[०-९]{3}-[०-९]{3}-[०-९]{4}$

            {
                return "Country code with dashes";
            }
            else if (Regex.IsMatch(phoneNumber, @"^\(\d{2,3}\)\d{10}$")||
                Regex.IsMatch(phoneNumber, @"^\+1 \(\d{3}\) \d{3}-\d{4}$") 
                ||Regex.IsMatch(phoneNumber, @"^\d{3}\) \d{3}-\d{4}$") || Regex.IsMatch(phoneNumber, @"^\(\d{3}\) \d{3}-\d{4}$"))

            {
                return "With parentheses for area code";
            }
            else
            {
                return "Other format";
            }
        }

        public class InputTextModel
        {
            public string Text { get; set; }
        }
        public class PhoneNumberInfo
        {
            public string Number { get; set; }
            public string Format { get; set; }
        }
    }

}

