using System.ComponentModel;

namespace Cryptogram.Models
{

    /// <summary>
    /// Credit card type.
    /// </summary>
    public enum CreditCardType
    {
        // Unknown issuers.
        [Description("Unknown Issuer")]
        Unknown,


        // American Express cards.
        [Description("American Express")]
        AmEx,


        // MasterCard cards.
        [Description("MasterCard")]
        MasterCard,

        // Visa cards.
        [Description("Visa")]
        Visa,

        /// <summary>
        /// Maestro cards.
        /// </summary>
        [Description("Maestro")]
        Maestro,

    }
}
