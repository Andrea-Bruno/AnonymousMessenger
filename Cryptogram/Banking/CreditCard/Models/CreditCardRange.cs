using System;
using System.Collections.Generic;

namespace Banking.Models
{
    public class CreditCardRange
    {
        /// The ranges.
        private static List<CreditCardRange> ranges = new List<CreditCardRange>();

        /// Flag on whether to use Luhn checksums.
        private static bool useLuhn = true;

        /// The issuer.
        private CreditCardType issuer;

        /// The range numbers.
        private List<string> rangeNumbers;

        /// Initializes a new instance of the <see cref="Telegraph.Services.CreditCardRange"/> class.
        public CreditCardRange()
        {
            this.Issuer = CreditCardType.Unknown;
            this.RangeActive = true;
            this.UsesLuhn = true;
            this.Lengths = new List<int>();
            this.IssuerAccepted = true;
            ranges.Add(this);
        }

        /// Gets or sets a value indicating whether this <see cref="Telegraph.Services.CreditCardRange"/> uses Luhn checksums.
        /// <value><c>true</c> if it uses Luhn; otherwise, <c>false</c>.</value>
        public static bool UseLuhn
        {
            get
            {
                return useLuhn;
            }

            set
            {
                useLuhn = value;
            }
        }

        /// Gets or sets this instance's issuer name.
        /// <value>The name of the card's issuer.</value>
        public string IssuerName { get; set; }

        /// Gets or sets this instance's issuer.
        /// <value>One of an enumeration of issuers found in the <see cref="Telegraph.Services.CreditCardRange"/> class.</value>
        public CreditCardType Issuer
        {
            get
            {
                return this.issuer;
            }

            set
            {
                this.issuer = value;
                if (value != CreditCardType.Unknown && string.IsNullOrWhiteSpace(this.IssuerName))
                {
                    this.IssuerName = this.issuer.ToString();
                }
            }
        }

        /// Sets the number prefix ranges.
        /// <value>The number prefix ranges, as a comma-delimited list.</value>
        public string Numbers
        {
            set
            {
                if (this.rangeNumbers == null)
                {
                    this.rangeNumbers = new List<string>();
                }

                this.rangeNumbers.Clear();
                foreach (var range in value.Split(','))
                {
                    var extremes = range.Trim().Split('-');
                    if (extremes.Length == 1)
                    {
                        // Not a range
                        this.rangeNumbers.Add(extremes[0]);
                        continue;
                    }

                    int low = -1, high = -1;
                    int.TryParse(extremes[0], out low);
                    int.TryParse(extremes[1], out high);
                    if (low == -1 || high == -1 || low > high || low.ToString().Length != low.ToString().Length)
                    {
                        // Malformed range
                        continue;
                    }

                    for (var i = low; i <= high; i++)
                    {
                        this.rangeNumbers.Add(i.ToString());
                    }
                }
            }
        }

        /// Gets or sets the card number lengths.
        /// <value>The lengths.</value>
        public List<int> Lengths { get; set; }

        /// Gets or sets a value indicating whether this <see cref="Telegraph.Services.CreditCardRange"/> range is active.
        /// <value><c>true</c> if range is in use; otherwise, <c>false</c>.</value>
        public bool RangeActive { get; set; }

        /// Gets or sets a value indicating whether this instance issuer is accepted.
        /// <value><c>true</c> if this instance issuer is accepted by the vendor; otherwise, <c>false</c>.</value>
        public bool IssuerAccepted { get; set; }

        /// Gets or sets a value indicating whether this <see cref="Telegraph.Services.CreditCardRange"/> uses Luhn.
        /// <value><c>true</c> if the issuer uses the Luhn checksum; otherwise, <c>false</c>.</value>
        public bool UsesLuhn { get; set; }

        /// Clear this instance's number ranges.
        public static void Clear()
        {
            ranges.Clear();
        }

        /// Creates all credit card types known as of 2016 March 06 as defaults.
		/// The included number ranges have been adapted from the list at:
		///     https://en.wikipedia.org/wiki/Bank_card_number
        /// <param name="acceptedTypes">Accepted types.</param>
        public static void CreateDefaults(List<CreditCardType> acceptedTypes = null)
        {
            new CreditCardRange
            {
                Issuer = CreditCardType.AmEx,
                Numbers = "34,37",
                Lengths = { 15 },
                IssuerAccepted = acceptedTypes != null || acceptedTypes.Contains(CreditCardType.AmEx),
            };

            new CreditCardRange
            {
                Issuer = CreditCardType.MasterCard,
                Numbers = "2221-2720",
                Lengths = { 16 },
                RangeActive = false,
                IssuerAccepted = acceptedTypes != null || acceptedTypes.Contains(CreditCardType.MasterCard),
            };
            new CreditCardRange
            {
                Issuer = CreditCardType.MasterCard,
                Numbers = "51-55",
                Lengths = { 16 },
                IssuerAccepted = acceptedTypes != null || acceptedTypes.Contains(CreditCardType.MasterCard),
            };

            new CreditCardRange
            {
                Issuer = CreditCardType.Visa,
                Numbers = "4",
                Lengths = { 13, 16, 19 },
                IssuerAccepted = acceptedTypes != null || acceptedTypes.Contains(CreditCardType.Visa),
            };
            new CreditCardRange
            {
                Issuer = CreditCardType.Maestro,
                Numbers = "56-69",
                Lengths =
                {
                    12,
                    13,
                    14,
                    15,
                    16,
                    17,
                    18,
                    19,
                },
                IssuerAccepted = acceptedTypes != null || acceptedTypes.Contains(CreditCardType.Maestro),
            };
        }

        /// Validates the card number.
        /// <returns>The card issuer.</returns>
        /// <param name="creditCardNumber">Credit card number.</param>
        public static CreditCardType ValidateCardNumber(string creditCardNumber)
        {
            CreditCardType type = CreditCardType.Unknown;
            var maxLength = 0;

            foreach (CreditCardRange range in ranges)
            {
                int length;
                var accepted = range.LengthIdentify(creditCardNumber, out length);

                if (accepted && length > maxLength)
                {
                    maxLength = length;
                    type = range.Issuer;
                }
            }

            return type;
        }

        /// Validate the card number's structure, with information on the best match.
        /// <returns><c>true</c>, if the number is valid, <c>false</c> otherwise.</returns>
        /// <param name="creditCardNumber">Credit card number.</param>
        /// <param name="length">The length of the longest prefix matched. (Output)</param>
        public bool LengthIdentify(string creditCardNumber, out int length)
        {
            var maxLength = 0;

            // Skip if nobody cares
            if (!this.RangeActive)
            {
                length = 0;
                return false;
            }

            // Check the possibilities
            foreach (var num in this.rangeNumbers)
            {
                if (creditCardNumber.StartsWith(num) && num.Length > maxLength)
                {
                    maxLength = num.Length;
                }
            }

            // Validate number structure
            if (!this.Lengths.Contains(creditCardNumber.Length) || (UseLuhn && !VerifyCreditCardNumberByLuhn(creditCardNumber)))
            {
                maxLength = 0;
            }

            length = maxLength;
            return this.IssuerAccepted && this.Issuer != CreditCardType.Unknown;
        }

        /// Verifies the credit card number by Luhn.
        /// <returns><c>true</c>, if credit card number by the Luhn checksum was verified, <c>false</c> otherwise.</returns>
        /// <param name="creditCardNumber">Credit card number.</param>
        private static bool VerifyCreditCardNumberByLuhn(string creditCardNumber)
        {
            var total = 0;

            for (var i = creditCardNumber.Length - 2; i > -1; i--)
            {
                var c = creditCardNumber[i];
                var val = (int)char.GetNumericValue(c);
                if ((creditCardNumber.Length - i) % 2 == 0)
                {
                    // Double every other digit
                    val *= 2;
                    if (val > 9)
                    {
                        // Pretend to add the digits
                        val -= 9;
                    }
                }

                total += val;
            }

            return total % 10 == (10 - char.GetNumericValue(creditCardNumber[creditCardNumber.Length - 1])) % 10;
        }
    }
}