using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace csh_wpf_ado_pg_northwind_import
{
    public class HostIpValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string hostInput = value.ToString().Trim();

            // localhost
            if (hostInput.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.ValidResult;
            }

            // IP address 
            // \b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b

            // regex verbatim (0.0.0.0 - 255.255.255.255)
            string regexPip = @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])(\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])){3}$";

            if (Regex.IsMatch(hostInput, regexPip))
            {
                return ValidationResult.ValidResult;
            }

            // FQDN 

            // ^(([a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6})$

            string regexPfqdn = @"^(([a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6})$";

            if (Regex.IsMatch(hostInput, regexPfqdn))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Input correct IP or FQDN");

        }
    }

    public class PortValidationRule : ValidationRule 
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value.ToString(), out int port))
            {
                if (port >= 1024 && port <= 65535)
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "Valid range 1024–65535");
            }
            return new ValidationResult(false, "Input correct integer");
        }
    }

    public class QuantityValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (!Regex.IsMatch(input, "^[0-9]+$"))
            {
                return new ValidationResult(false, "Only numbers allowed");
            }
            return ValidationResult.ValidResult;
        }
    }
}
