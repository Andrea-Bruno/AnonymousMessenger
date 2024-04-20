using System;
using System.Collections.Generic;
using System.Text;

namespace SecureKeyboard
{
    class KeyboardHelper
    {
        public static List<string> GetEmojiList()
        {
            var emojiList = new List<string>
            {
                "\U0001F600",
                "\U0001F603",
                "\U0001F604",
                "\U0001F601",
                "\U0001F642",
                "\U0001F643",
                "\U0001F609",
                "\U0001F60A",
                "\U0001F607",
                "\U0001F606",
                "\U0001F605",
                "\U0001F602",
                "\U0001F923",

                //sad
                "\U0001F60C",
                "\U0001F614",
                "\U0001F62A",
                "\U0001F924",
                "\U0001F634",
                "\U0001F615",
                "\U0001F61F",
                "\U0001F941",
                "\U0001F62E",
                "\U0001F62F",
                "\U0001F633",
                "\U0001F627",
                "\U0001F628",
                "\U0001F625",
                "\U0001F622",
                "\U0001F616",


                "\U0001F60D",
                "\U0001F929",
                "\U0001F618",
                "\U0001F617",
                "\U0001F61A",
                "\U0001F619",
                "\U0001F60B",

                //dil
                "\U0001F61B",
                "\U0001F61C",
                "\U0001F61D",
                "\U0001F911",

                //face-hand
                "\U0001F917",
                "\U0001F92D",
                "\U0001F92B",
                "\U0001F914",

                //face neutral
                "\U0001F910",
                "\U0001F928",
                "\U0001F610",
                "\U0001F611",
                "\U0001F636",
                "\U0001F60F",
                "\U0001F612",
                "\U0001F644",
                "\U0001F62C",

                //unwell
                "\U0001F637",
                "\U0001F912",
                "\U0001F915",
                "\U0001F62E",
                "\U0001F927",
                "\U0001F60E",

                //heart
                "\U0001F48B",
                "\U0001F48C",
                "\U0001F498",
                "\U0001F49D",
                "\U0001F496",
                "\U0001F497",
                "\U0001F493",
                "\U0001F494"
            };

            return emojiList;
        }

        public static List<List<string>> GetKeys()
        {
            var keys = new List<List<string>>();

            var firstLineKeys = new List<string>
            {
                "QWERTYUIOP",
                "qwertyuiop",
                "1234567890",
                "£€¥¢©®™~¿["
            };
            keys.Add(firstLineKeys);

            var secondLineKeys = new List<string>
            {
                "ASDFGHJKL",
                "asdfghjkl",
                "!@#$%^&*(",
                "\"]<>÷¡¬¦|"
            };
            keys.Add(secondLineKeys);

            var thirdLineKeys = new List<string>
            {
                "⇮ZXCVBNM⌫",
                "⇯zxcvbnm⌫",
                "❸)_-+={}⌫",
                "❶';°¶§×\\⌫"
            };
            keys.Add(thirdLineKeys);

            var fourthLineKeys = new List<string>
            {
                "❶☺, ?.⏎✓",
                "❶☺, ?.⏎✓",
                "❷☺, ?.⏎✓",
                "❷☺, ?.⏎✓"
            };
            keys.Add(fourthLineKeys);
            return keys;
        }
    }
}
