using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CALCELIG
{
    class Eligibility
    {
        public static string converthcid(string hcid)
        {
            string Hcid = hcid;
            if (Hcid.Count() < 9)
            {
                Hcid = "";
            }
            else
            {

                string prealpha = Hcid.Substring(0, 3);
                string alpha = Hcid.Substring(3, 1);
                string postalpha = Hcid.Substring(4, 5);

                if (alpha == "A" || alpha == "B" || alpha == "C")
                {
                    Hcid = alpha + prealpha + '2' + postalpha;
                }
                else if (alpha == "D" || alpha == "E" || alpha == "F")
                {
                    Hcid = alpha + prealpha + '3' + postalpha;
                }
                else if (alpha == "G" || alpha == "H" || alpha == "I")
                {
                    Hcid = alpha + prealpha + '4' + postalpha;
                }
                else if (alpha == "J" || alpha == "K" || alpha == "L")
                {
                    Hcid = alpha + prealpha + '5' + postalpha;
                }
                else if (alpha == "M" || alpha == "N" || alpha == "O")
                {
                    Hcid = alpha + prealpha + '6' + postalpha;
                }
                else if (alpha == "P" || alpha == "Q" || alpha == "R" || alpha == "S")
                {
                    Hcid = alpha + prealpha + '7' + postalpha;
                }
                else if (alpha == "T" || alpha == "U" || alpha == "V")
                {
                    Hcid = alpha + prealpha + '8' + postalpha;
                }
                else if (alpha == "W" || alpha == "X" || alpha == "Y" || alpha == "Z")
                {
                    Hcid = alpha + prealpha + '9' + postalpha;
                }
            }

            return Hcid;
        }

        public static string DobFormat(string dob)
        {
            string [] DOBUnit = dob.Split('/');

            string MM = DOBUnit[0].PadLeft(2,'0');

            string DD = DOBUnit[1].PadLeft(2, '0');

            string YYYY = DOBUnit[2];

            dob = MM + "/" + DD + "/" + YYYY;

            return dob;
        }

        public static string ToTitleCase(string ToCase)
        {
            string TCase = ToCase.ToLower();
            ToCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(TCase);
            return ToCase;
        }
    }
}
