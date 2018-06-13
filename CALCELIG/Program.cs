using System;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data.OleDb;
using System.Data;
using System.IO;
using Minimod.Impersonator;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CALCELIG
{
    class Program
    {


        static async Task Main(string[] args)
        {
            string EmailTitle = "";
            string EmailSubTitle = "";
            string recipients = "";
            string FileName = "";
            string EmailBody = "";
            string eligibilityFiles = DateTime.Now.ToString("MMyyyy");
  
            FileName = "Carrier_Extract_" + eligibilityFiles + ".xls";
            if (File.Exists(@"\\pswadmin.com\corp\Share\E-Exchange\PSWA\Eligibility\QR\Outbound\" + FileName))
            {
                string line, lineFeed, dline, mline, fields, mem, healthBenefitCode, elig, coverageTypeCode, EligKey, dupelig;
                string identity, eligClassCode, reTypeCode, identifierCode, sbcId, mstr, EligOver, ext;
                string CarrierExtract = "";
                string q = "";
                int i = 0, x = 0;
               
                var lines = new List<string>();
                var mlines = new List<string>(); 
                var elines = new List<string>();
                var mstlines = new List<string>();
                var extlines = new List<string>();
                var dupeliglines = new List<string>();
                var extractlines = new List<string>();

                List<string> list_masterRecord = new List<string>();
                List<string> list_temp = new List<string>();
                List<string> error_log = new List<string>();
                Dictionary<string, string> dict_Hcid = new Dictionary<string, string>();
                Dictionary<string, string> dict_dup = new Dictionary<string, string>();
                Dictionary<string, string> dict_masterRec = new Dictionary<string, string>();
                Dictionary<string, string> dict_mapping = new Dictionary<string, string>();
                Dictionary<string, string> dict_elig_class = new Dictionary<string, string>();
                Dictionary<string, string> dict_coverage_type = new Dictionary<string, string>();
                Dictionary<string, string> dict_coverage_key = new Dictionary<string, string>();
                Dictionary<string, string> dict_relationship_type = new Dictionary<string, string>();
                Dictionary<string, string> dict_sbc = new Dictionary<string, string>();
                List<KeyValuePair<string, string>> list_identity = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> list_extract = new List<KeyValuePair<string, string>>();
                Dictionary<string, int> dups = new Dictionary<string, int>();
                Dictionary<string, int> dups_elig = new Dictionary<string, int>();
                Dictionary<string, int> eodups = new Dictionary<string, int>();
                Dictionary<string, string> dict_Rel = new Dictionary<string, string>();
                Dictionary<string, string> dict_CovType = new Dictionary<string, string>();
                Dictionary<string, string> dict_Sbc = new Dictionary<string, string>();
                Dictionary<string, string> dict_Hbp = new Dictionary<string, string>();
                Dictionary<string, string> dict_Fund = new Dictionary<string, string>();
                Dictionary<string, string> dict_EClass = new Dictionary<string, string>();


                string now = DateTime.Now.ToString("MMddyyyyHHmmss");

                DirectoryInfo inboundFiles = new DirectoryInfo(@"\\pswadmin.com\corp\Share\E-Exchange\PSWA\Eligibility\QR\Outbound\");
                DirectoryInfo ArchiveDestination = new DirectoryInfo(@"\\pswadmin.com\corp\Share\E-Exchange\PSWA\Eligibility\QR\Outbound\Archive\");
                DirectoryInfo target = new DirectoryInfo(@"C:\PSWA\Loads\QR\Eligibility\Incoming\");
                using (new Impersonator("!mmorales", "pswadmin", "Selarom02"))
                {
                    
                    foreach (FileInfo fi in inboundFiles.GetFiles())
                    {
                        if (fi.Length != 0)
                        fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

                        File.Move(@"\\pswadmin.com\corp\Share\E-Exchange\PSWA\Eligibility\QR\Outbound\" + fi.Name, @"\\pswadmin.com\corp\Share\E-Exchange\PSWA\Eligibility\QR\Outbound\Archive\" + fi.Name + "_" + now);

                    }

                }



                // Read the file and display it line by line
                System.IO.StreamReader MasterRecord =
                    new System.IO.StreamReader("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv");
                while ((line = MasterRecord.ReadLine()) != null)
                {
                    var values = line.Split('|');

                    string MemberSNN = values[1].Trim();
                    string FirstName = values[2].Trim().ToLower();
                    string Dob = Eligibility.DobFormat(values[4]);
                    dict_dup.Add(MemberSNN + Dob + FirstName.ToLower(), line);

                }
                MasterRecord.Close();
                System.IO.StreamReader CovFile =
                    new System.IO.StreamReader(@"C:\Program Files\CALCELIG\SYS\Dependencies\CoverageType.csv");
                while ((line = CovFile.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    dict_CovType.Add(values[1], values[0]);

                }
                CovFile.Close();

                System.IO.StreamReader RelFile =
                    new System.IO.StreamReader(@"C:\Program Files\CALCELIG\SYS\Dependencies\RelationshipBasys.csv");
                while ((line = RelFile.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    dict_Rel.Add(values[1], values[0]);
                }
                RelFile.Close();

                System.IO.StreamReader Sbc =
                    new System.IO.StreamReader(@"C:\Program Files\CALCELIG\SYS\Dependencies\Sbc.csv");
                while ((line = Sbc.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    dict_Sbc.Add(values[1] + values[0], values[0]);
                }
                Sbc.Close();
                System.IO.StreamReader Hbp =
                    new System.IO.StreamReader(@"C:\Program Files\CALCELIG\SYS\Dependencies\Sbc.csv");
                while ((line = Hbp.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    dict_Hbp.Add(values[1] + values[2], values[0]);
                }
                Hbp.Close();
                System.IO.StreamReader Fund =
                    new System.IO.StreamReader(@"C:\Program Files\CALCELIG\SYS\Dependencies\Fund.csv");
                while ((line = Fund.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    dict_Fund.Add(values[1], values[0]);
                }
                Fund.Close();

                System.IO.StreamReader EClass =
                    new System.IO.StreamReader(@"C:\Program Files\CALCELIG\SYS\Dependencies\EligibilityClass.csv");
                while ((line = EClass.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    dict_EClass.Add(values[1], values[0]);
                }
                EClass.Close();

                using (TextFieldParser parser = new TextFieldParser(@"C:\PSWA\Loads\QR\Eligibility\Incoming\2yr-elig\24Month.csv"))
                {
                    parser.Delimiters = new string[] { "|" };
                    while (!parser.EndOfData)
                    {
                        var itemRecord = parser.ReadFields().ToList<string>();
                        var MemberSsn = itemRecord[0].Trim().Replace("-", "");
                        var Dob = Eligibility.DobFormat(itemRecord[7]);
                        var _Relationship = Eligibility.ToTitleCase(itemRecord[5]);
                        string[] Name = itemRecord[4].ToLower().Split(' ');


                        string FirstName = "";
                        string LastName = "";
                        if (Name.Count() == 2)
                        {
                            FirstName = Name[0];
                            LastName = Name[1];
                        }
                        else if (Name.Count() == 3)
                        {
                            FirstName = Name[0];
                            LastName = Name[1] + " " + Name[2];
                        }
                        else if (Name.Count() == 4)
                        {
                            FirstName = Name[0];
                            LastName = Name[1] + " " + Name[2] + " " + Name[3];
                        }
                        else if (Name.Count() == 5)
                        {
                            FirstName = Name[0];
                            LastName = Name[1] + " " + Name[2] + " " + Name[3] + " " + Name[4];
                        }
                        else if (Name.Count() == 6)
                        {
                            FirstName = Name[0];
                            LastName = Name[1] + " " + Name[2] + " " + Name[3] + " " + Name[4] + " " + Name[5];
                        }

                        string UID = MemberSsn + Dob + FirstName;

                        if (dict_dup.ContainsKey(UID))
                        {
                            string[] ExtractLogValue = dict_dup[UID].Split('|');
                            string Extract = ExtractLogValue[0] + '|' + ExtractLogValue[1] + '|' + ExtractLogValue[2] + '|' + ExtractLogValue[3] + '|' + ExtractLogValue[4] + '|' + ExtractLogValue[5] + '|' + ExtractLogValue[6] + "|" + ExtractLogValue[7] + "|" + ExtractLogValue[8] + "|" + ExtractLogValue[9] + "|" + ExtractLogValue[10] + "|" + ExtractLogValue[11] + "|" + ExtractLogValue[12] + "|" + ExtractLogValue[13] + "|" + ExtractLogValue[14] + "|3|" + itemRecord[3];
                            dict_dup[UID] = Extract;

                        }
                        else if (itemRecord[7] == "")
                        {
                            Console.WriteLine("This record does not have DOB XXXXXXXXXXXXXXXXXX" + UID);
                            continue;
                        }
                        else
                        {
                            string EligibilityClass = "9999";

                            string Hcid = "";
                            Hcid = Eligibility.converthcid(itemRecord[1]);

                            if (dict_dup.Count > 0)
                            {

                                string[] LastValue = dict_dup.Values.Last().Split('|');
                                int r = Int32.Parse(LastValue[0]) + 1;
                                q = r.ToString();
                            }
                            else
                            {
                                q = "1";
                            }


                            string newlist = q + '|' + itemRecord[0] + '|' + FirstName + '|' + LastName + '|' + Dob + '|' + itemRecord[6] + '|' + _Relationship + "||||||" + EligibilityClass + "|" + Hcid + "||3|" + itemRecord[3];


                            // list_masterRecord.Add(newlist);
                            dict_dup.Add(UID, newlist);

                        }

                    }
                }
               // System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv", list_masterRecord.ToArray());


                // Read the file and display it line by line
                System.IO.StreamReader HcidFile =
                new System.IO.StreamReader("C:\\PSWA\\Loads\\BCBS\\Inbound\\hcid.txt");
                while ((line = HcidFile.ReadLine()) != null)
                {
                    var values = line;

                    string PreLineThree = values.Substring(0, 3);
                    if (PreLineThree == "EMP")
                    {
                        string HcidSubscriberSsn = values.Substring(3, 9);
                        string Hcid = values.Substring(300, 9);




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
                        else
                        {
                            continue;
                        }
                        if (dict_Hcid.ContainsKey(HcidSubscriberSsn))
                        {
                            continue;
                        }
                        else
                        {
                            dict_Hcid.Add(HcidSubscriberSsn, Hcid);
                        }
                    }
                    else
                    {
                        continue;
                    }


                }
                HcidFile.Close();
                string con =
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source= C:\PSWA\Loads\QR\Eligibility\Incoming\" + FileName +";" +
                @"Extended Properties='Excel 8.0;HDR=Yes;'";
                using (OleDbConnection connection = new OleDbConnection(con))
                {
                    connection.Open();
                    // Get the name of the first worksheet:

                    DataTable dbSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    var totalSheets = dbSchema.Rows.Count;
                    if (dbSchema == null || dbSchema.Rows.Count < 1)
                    {
                        throw new Exception("Error: Could not determine the name of the first worksheet.");
                    }

                    for (int recordCount = 0; recordCount < totalSheets; ++recordCount)
                    {
                        string firstSheetName = dbSchema.Rows[recordCount]["TABLE_NAME"].ToString();

                        // Now we have the table name; proceed as before:
                        OleDbCommand command = new OleDbCommand("SELECT * FROM [" + firstSheetName + "]", connection);

                        using (OleDbDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {

                                var EmployeeSSN = dr[0];
                                var MemberSSN = dr[1];
                                var LastName = dr[2];
                                var FirstName = dr[3];
                                var MiddleName = dr[4];
                                var Relationship = dr[5];
                                var Gender = dr[6];
                                var Dob = String.Format("{0:MM/dd/yyyy }", dr[7]);
                                var EligibilityPeriod = String.Format("{0:MM/dd/yyyy }", dr[8]);
                                var CarrierID = dr[9];
                                var CarrierName = dr[10];
                                var HealthBenefitPlan = dr[11];
                                var BenefitGroup = dr[12];
                                var CoverageType = dr[13];
                                var EligibilityClass = dr[14];
                                var CoverageId = dr[15];
                                var AddressLine1 = dr[16];
                                var AddressLine2 = dr[17];
                                var City = dr[18];
                                var State = dr[19];
                                var Zip = dr[20];

                                string SubcriberSnn = EmployeeSSN.ToString();
                                string _SubscriberSnn = SubcriberSnn.Replace("-", "").Trim();
                                string _Dob = Dob.ToString().Trim();
                                string _FirstName = FirstName.ToString().ToLower().Trim();
                                string _MiddleName = MiddleName.ToString().ToLower().Trim();
                                string _LastName = LastName.ToString().ToLower().Trim();
                                string _MemberSSN = MemberSSN.ToString().Replace("-", "");
                                string ExtractUID = _SubscriberSnn + _Dob + _FirstName.ToLower();
                                string _EligibilityClass = EligibilityClass.ToString();

                                if (Dob == "")
                                {
                                    Console.WriteLine("Missing Date of Birth");
                                    continue;
                                }

                                if (dict_EClass.ContainsKey(_EligibilityClass))
                                {
                                    EligibilityClass = dict_EClass[_EligibilityClass];
                                }
                                else
                                {
                                    Console.WriteLine("Fatal Error: Can not find Eligibility class");
                                }
                                string hcid = "";
                                if (dict_Hcid.ContainsKey(_SubscriberSnn))
                                {
                                    hcid = dict_Hcid[_SubscriberSnn];
                                }

                                if (dict_dup.ContainsKey(ExtractUID))
                                {
                                    string[] ExtractedValue = dict_dup[ExtractUID].Split('|');
                                    CarrierExtract = ExtractedValue[0] + "|" + _SubscriberSnn + "|" + _FirstName + "|" + _MiddleName + " " + _LastName + "|" + _Dob + "|" + Gender + "|" + Relationship + "|" + AddressLine1 + "|" + AddressLine2 + "|" + City + "|" + State + "|" + Zip + "|" + EligibilityClass + "|" + ExtractedValue[13] + "|" + _MemberSSN + "|6|" + ExtractedValue[16];
                                    dict_dup[ExtractUID] = CarrierExtract;
                                }
                                else
                                {
                                    string[] LastValue = dict_dup.Values.Last().Split('|');
                                    int r = Int32.Parse(LastValue[0]) + 1;
                                    q = r.ToString();
                                    CarrierExtract = q + "|" + _SubscriberSnn + "|" + _FirstName + "|" + _MiddleName + " " + _LastName + "|" + _Dob + "|" + Gender + "|" + Relationship + "|" + AddressLine1 + "|" + AddressLine2 + "|" + City + "|" + State + "|" + Zip + "|" + EligibilityClass + "|" + hcid + "|" + _MemberSSN + "|6|" + "9999";
                                    dict_dup.Add(ExtractUID, CarrierExtract);

                                }

                                string CarrierExtractLines = EmployeeSSN + "|" + MemberSSN + "|" + _LastName + "|" + _FirstName + "|" + _MiddleName + "|" + Relationship + "|" + Gender + "|" + _Dob + "|" + EligibilityPeriod + "|" + CarrierID + "|" + CarrierName + "|" + HealthBenefitPlan + "|" + BenefitGroup + "|" + CoverageType + "|" + _EligibilityClass + "|" + CoverageId + "|" + AddressLine1 + "|" + AddressLine2 + "|" + City + "|" + State + "|" + Zip + "|333";
                                extractlines.Add(CarrierExtractLines);
                                Console.WriteLine(CarrierExtractLines);
                            }
                        }
                    }
                }
                var EligList = dict_dup.Values.ToList();

                System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\extract.csv", extractlines.ToArray());
                System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv", EligList.ToArray());


                System.IO.StreamReader virtualRecord =
                new System.IO.StreamReader("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv");
                while ((line = virtualRecord.ReadLine()) != null)
                {
                    string itemRecord = line;


                    list_masterRecord.Add(itemRecord);
                }
                virtualRecord.Close();


                using (TextFieldParser parser = new TextFieldParser("C:\\PSWA\\Loads\\QR\\Eligibility\\extract.csv"))
                {
                    parser.Delimiters = new string[] { "|" };
                    while (!parser.EndOfData)
                    {
                        var itemRecord = parser.ReadFields().ToList<string>();
                        var MemberSsn = itemRecord[0].Trim().Replace("-", "");
                        var Dob = itemRecord[7];
                        var FirstName = itemRecord[3].ToLower();

                        var ExtractUID = MemberSsn + Dob + FirstName;

                        if (dict_dup.ContainsKey(ExtractUID))
                        {
                            continue;
                        }
                        else if (itemRecord[7] == "")
                        {
                            Console.WriteLine("This record does not have DOB XXXXXXXXXXXXXXXXXX" + ExtractUID);
                            error_log.Add("This record does not contain DOB " + MemberSsn +  "|" + FirstName);
                            continue;
                        }
                        else
                        {
                            string EligibilityClass = "";
                            string EligibilityInd = itemRecord[14];
                            if (dict_EClass.ContainsKey(EligibilityInd))
                            {
                                EligibilityClass = dict_EClass[itemRecord[14]];
                            }
                            else
                            {
                                EligibilityClass = "";
                            }
                            string Hcid = "";
                            if (dict_Hcid.ContainsKey(MemberSsn))
                            {
                                Hcid = dict_Hcid[MemberSsn];
                            }
                            else
                            {
                                Hcid = "";
                            }
                            string Relationship = "";
                            if (dict_Rel.ContainsKey(itemRecord[5]))
                            {
                                Relationship = dict_Rel[itemRecord[5]];
                            }
                            var LastValue = dict_dup.Values.Last();
                            int r = Int32.Parse(LastValue) + 1;
                            q = r.ToString();

                            string newlist = q + '|' + itemRecord[0].Replace("-", "") + '|' + itemRecord[3] + '|' + itemRecord[2] + '|' + itemRecord[7] + '|' + itemRecord[6] + '|' + Relationship + '|' + itemRecord[16] + '|' + itemRecord[18] + '|' + itemRecord[19] + '|' + itemRecord[20] + '|' + EligibilityClass + "|" + Hcid + "|" + itemRecord[1].Replace("-", "") + "|6";


                            list_masterRecord.Add(newlist);
                            dict_dup.Add(ExtractUID, q);

                        }

                    }
                }
                System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv", list_masterRecord.ToArray());




                //using (new Impersonator("sqladmin", "selarom", "5Trgw7B@56Z"))
                //{
                //    System.IO.File.WriteAllLines(@"\\lvshare01\\Company\\Share\\PSWA\\Loads\\LB\\Eligibility\\Outgoing\\Master Record\\master_file_final.csv", list_masterRecord.ToArray());

                //}





                    System.IO.StreamReader masterRecord =
               new System.IO.StreamReader("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv");
                while ((line = masterRecord.ReadLine()) != null)
                {
                    var mValues = line.Split('|');

                    var mID = mValues[1].PadLeft(9, '0') + mValues[2].ToLower() + mValues[4].Replace("/", "");
                    dict_masterRec.Add(mID, mValues[0]);
                }
                masterRecord.Close();
                //Read the file and display it line by line
                System.IO.StreamReader mapping =
                    new System.IO.StreamReader("C:\\Program Files\\CALCELIG\\SYS\\Dependencies\\HealthBenefitPlan.csv");
                while ((line = mapping.ReadLine()) != null)
                {
                    var values = line.Split('|');
                    if (dict_mapping.ContainsKey(values[1].Trim() + values[2]))
                    {
                        Console.WriteLine(values[1] + values[2]);
                    }
                    else
                    {
                        dict_mapping.Add(values[1].Trim() + values[2], values[0]);
                    }
                }
                mapping.Close();


                // Read the file and display it line by linemReader 
                System.IO.StreamReader elig_class =
                    new System.IO.StreamReader("C:\\Program Files\\CALCELIG\\SYS\\Dependencies\\EligibilityClass.csv");
                while ((line = elig_class.ReadLine()) != null)
                {
                    var ecvalues = line.Split('|');
                    dict_elig_class.Add(ecvalues[0], ecvalues[1]);
                }
                elig_class.Close();
                // Read the file and display it line by line
                System.IO.StreamReader coverage_type =
                    new System.IO.StreamReader("C:\\Program Files\\CALCELIG\\SYS\\Dependencies\\CoverageType.csv");
                while ((line = coverage_type.ReadLine()) != null)
                {
                    var ctvalues = line.Split('|');
                    dict_coverage_type.Add(ctvalues[1], ctvalues[0]);
                }
                coverage_type.Close();

                // Read the file and display it line by line
                System.IO.StreamReader coverage_key =
                    new System.IO.StreamReader("C:\\Program Files\\CALCELIG\\SYS\\Dependencies\\CoverageTypeQR.csv");
                while ((line = coverage_key.ReadLine()) != null)
                {
                    var ckvalues = line.Split('|');
                    dict_coverage_key.Add(ckvalues[1], ckvalues[0]);
                }
                coverage_key.Close();
                dict_relationship_type.Clear();
                // Read the file and display it line by line
                System.IO.StreamReader relationship_type =
                new System.IO.StreamReader("C:\\Program Files\\CALCELIG\\SYS\\Dependencies\\RelationshipTbt.csv");
                while ((line = relationship_type.ReadLine()) != null)
                {
                    var reType = line.Split('|');
                    dict_relationship_type.Add(reType[0], reType[1]);
                }
                relationship_type.Close();
                // Read the file and display it line by line
                System.IO.StreamReader sbc_pointer =
                    new System.IO.StreamReader("C:\\Program Files\\CALCELIG\\SYS\\Dependencies\\Sbc.csv");
                while ((line = sbc_pointer.ReadLine()) != null)
                {
                    var sbcPin = line.Split('|');
                    dict_sbc.Add(sbcPin[0], sbcPin[1]);
                }
                sbc_pointer.Close();


                // Read the file and display it line by line
                System.IO.StreamReader extractPhaseTwo =
                new System.IO.StreamReader("C:\\PSWA\\Loads\\QR\\Eligibility\\master_file_final.csv");
                while ((line = extractPhaseTwo.ReadLine()) != null)
                {
                    var extractFinal = line.Split('|');

                    //if (i == 0)
                    //{
                    //    identity = "Id,AltId,SubscriberSsn,MemberSsn,FundId";
                    //}
                    //else
                    //{
                    string Bcid = "";
                    if (extractFinal[13] == "")
                    {
                        Bcid = "";
                    }
                    else
                    {
                        Bcid = extractFinal[13];
                    }
                    string MemberSsn = "";
                    if (extractFinal[14] == "")
                    {
                        MemberSsn = "";
                    }
                    else
                    {
                        MemberSsn = "";
                    }
                    identity = extractFinal[0] + "|" + Bcid + "|" + "6" + '|' + extractFinal[1].PadLeft(9, '0') + extractFinal[16].PadLeft(3, '0') + '|' + extractFinal[1].PadLeft(9, '0');

                    reTypeCode = "";
                    if (dict_Rel.ContainsKey(extractFinal[6].Trim()))
                    {
                        reTypeCode = dict_Rel[extractFinal[6].Trim()];
                    }



                    eligClassCode = extractFinal[11];


                    //if (i == 0)
                    //{
                    //    mem = "Id|FirstName|LastName|Dob|RelationshipId|EligibilityClassId";
                    //}
                    //else
                    //{
                    //string[] formats = { "MM/dd/yyyy", "M/d/yyyy", "M/d/yyyy" };
                    //string converted = DateTime.ParseExact(extractFinal[8], formats, CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("MM/dd/yyyy");
                    mem = extractFinal[0] + "|" + extractFinal[7] + "|" + extractFinal[12] + "|" + extractFinal[4] + '|' + eligClassCode + '|' + extractFinal[2].Replace(",", String.Empty) + '|' + extractFinal[3].Replace(",", String.Empty) + '|' + reTypeCode + '|' + extractFinal[9] + '|' + extractFinal[10];
                    //}
                    i++;
                    lines.Add(identity);
                    mlines.Add(mem);
                }


                extractPhaseTwo.Close();

                System.IO.File.WriteAllLines(@"C:\\PSWA\\Loads\\QR\\Eligibility\\Outgoing\\Identity_QR.csv", lines.ToArray());
                System.IO.File.WriteAllLines(@"C:\\PSWA\\Loads\\QR\\Eligibility\\Outgoing\\User_QR.csv", mlines.ToArray());
                //using (new Impersonator("sqladmin", "selarom", "5Trgw7B@56Z"))
                //{

                //    System.IO.File.WriteAllLines(@"\\lvshare01\\Company\\Share\\PSWA\\Loads\\LB\\Outgoing\\Eligibility\\Identity_LB.csv", lines.ToArray());
                //    System.IO.File.WriteAllLines(@"\\lvshare01\\Company\\Share\\PSWA\\Loads\\LB\\Outgoing\\Eligibility\\User_LB.csv", mlines.ToArray());
                //}


                // Read the file and display it line by line
                System.IO.StreamReader identity_pointer =
                new System.IO.StreamReader("C:\\PSWA\\Loads\\QR\\Eligibility\\Outgoing\\Identity_QR.csv");
                while ((line = identity_pointer.ReadLine()) != null)
                {
                    var identityPin = line.Split('|');
                    list_identity.Add(new KeyValuePair<string, string>(identityPin[3].PadLeft(9, '0'), identityPin[0]));
                }
                identity_pointer.Close();

                // Read the file and display it line by line
                using (TextFieldParser parserTwo = new TextFieldParser("C:\\PSWA\\Loads\\QR\\Eligibility\\extract.csv"))
                {
                    EligKey = "";
                    int e = 0;
                    parserTwo.Delimiters = new string[] { "|" };
                    while (!parserTwo.EndOfData)
                    {
                        var extractL = parserTwo.ReadFields().ToList<string>();

                        string[] _eligMonth = extractL[8].Split('/');

                        string eligMonth = _eligMonth[0] + "/" + _eligMonth[2];

                        string CompareEligMonth = _eligMonth[0] + _eligMonth[2];

                        if (CompareEligMonth != eligibilityFiles)
                        {
                            EmailTitle = "Painting and Drywall eligibility load";
                            EmailSubTitle = "First Phase Failed";
                            EmailBody = "This is an automated notification. The first phase has failed due to an incorrect eligibility month of " + eligMonth + ", File should have " + eligibilityFiles +   " month. " + FileName + " has failed first phase";
                            recipients = "mmorales@selarom.us|support@pswadmin.com";
                            await APIRequest(EmailTitle, EmailSubTitle, EmailBody, recipients);
                            Environment.Exit(0);
                        }

                        fields = extractL[0].Replace("-", "") + extractL[3].ToLower() + extractL[7].Replace("/", "") + extractL[13].ToLower();
                        identifierCode = "";
                        sbcId = "";
                        if (dups.ContainsKey(fields))
                        {
                            x++;
                            Console.WriteLine("Duplicate Member/Health Plan");
                            continue;
                        }
                        else
                        {
                            dups.Add(fields, 1);  
                            if(dict_coverage_key[extractL[13]] == null)
                            {
                                Console.WriteLine(extractL[13]);
                                continue;
                            }
                            string HealthPlan = extractL[11].Trim() + dict_coverage_key[extractL[13]];
                            if (dict_mapping.ContainsKey(HealthPlan))
                            {
                                healthBenefitCode = dict_mapping[HealthPlan];
                            }
                            else
                            {
                                Console.WriteLine("Fatal Error no HealthPlan match: " + HealthPlan + " " + extractL[0] + " " + extractL[3]);
                                error_log.Add("Error no HealthPlan match " + extractL[3] + "|" + extractL[0] + "|" + HealthPlan);

                                healthBenefitCode = "";
                                continue;
                            }

                            //Start creating eligibility rows

                            string identifier = extractL[0].Replace("-", "") + extractL[3].ToLower() + extractL[7].Replace("/", "");
                            if (dict_masterRec.ContainsKey(identifier))
                            {
                                EligKey = dict_masterRec[identifier];
                            }
                            else
                            {
                                Console.WriteLine("Fatal Error Could not find a match:" + " " + extractL[0] + " " + extractL[3]);
                                continue;
                            }



                            if (dict_coverage_key.ContainsKey(extractL[13]))
                            {
                                coverageTypeCode = dict_coverage_key[extractL[13]];
                            }
                            else
                            {
                                coverageTypeCode = "";
                            }

                            int coverageType = int.Parse(coverageTypeCode);
                            if (extractL[11] != "")
                            {
                                string HealthBenefitId;
                                HealthBenefitId = healthBenefitCode;
                                string BenefitGroup = extractL[12].TrimEnd();

                                string Flat = extractL[14];
                                var Jivis = extractL[14].Split(' ');
                                switch (coverageType)
                                {
                                    case 1:
                                        //MEDICAL PLAN A
                                        if (HealthBenefitId == "103")
                                        {
                                            //QR_MEDICAL_PLAN_A.docx
                                                    sbcId = "62";                                                                 
                                        }
                                        //MEDICAL PLAN B
                                        else if (HealthBenefitId == "105")
                                        {
                                            //QR_MEDICAL_PLAN_B.docx
                                            sbcId = "64";
                                        }
                                        //MEDICAL PLAN C
                                        else if (HealthBenefitId == "107")
                                        {
                                            //QR_MEDICAL_PLAN_C.docx
                                            sbcId = "65";
                                        }
                                        //MEDICAL PLAN D
                                        else if (HealthBenefitId == "130")
                                        {
                                            //QR_MEDICAL_PLAN_C.docx
                                            sbcId = "66";
                                        }
                                        //MEDICAL KAISER PLAN A MRV
                                        else if (HealthBenefitId == "117")
                                        {
                                            //Kaiser Plan A MRV
                                            sbcId = "70";
                                        }
                                        //MEDICAL KAISER PLAN B MRV
                                        else if (HealthBenefitId == "120")
                                        {
                                            //Kaiser Plan B MRV
                                            sbcId = "71";
                                        }
                                        //MEDICAL KAISER PLAN C MR
                                        else if (HealthBenefitId == "121")
                                        {
                                            //Kaiser Plan C MR
                                            sbcId = "72";
                                        }
                                        //MEDICAL KAISER PLAN D MR
                                        else if (HealthBenefitId == "123")
                                        {
                                            //Kaiser Plan D MR
                                            sbcId = "73";
                                        }
                                        //MEDICAL AETNA PLAN A 
                                        else if (HealthBenefitId == "97")
                                        {
                                            //AETNA  Plan A
                                            sbcId = "74";
                                        }
                                        //MEDICAL AETNA PLAN B
                                        else if (HealthBenefitId == "99")
                                        {
                                            //AETNA  Plan B
                                            sbcId = "75";
                                        }
                                        //MEDICAL AETNA PLAN C
                                        else if (HealthBenefitId == "101")
                                        {
                                            //AETNA  Plan C
                                            sbcId = "76";
                                        }
                                        //MEDICAL AETNA PLAN D
                                        else if (HealthBenefitId == "102")
                                        {
                                            //AETNA  Plan D
                                            sbcId = "77";
                                        }
                                        break;

                                    case 2:
                                        //Dental
                                        //DELTA DENTAL
                                        if (HealthBenefitId == "108")
                                        {
                                            //DELTA DENTAL
                                            sbcId = "78";
                                        }
                                        //DELTA PLAN A 
                                        else if (HealthBenefitId == "109")
                                        {
                                            //DELTA PLAN A 
                                            sbcId = "79";
                                        }
                                        //DELTACARE
                                        else if (HealthBenefitId == "110")
                                        {
                                            //DELTACARE
                                            sbcId = "80";
                                        }
                                        //DeltaCare PLAN A Dental
                                        else if (HealthBenefitId == "111")
                                        {
                                            //DELTACARE PLAN A
                                            sbcId = "81";
                                        }
                                        //DeltaCare PLAN B Dental
                                        else if (HealthBenefitId == "112")
                                        {
                                            //DELTACARE PLAN B
                                            sbcId = "82";
                                        }
                                        //DeltaCare PLAN D Dental
                                        else if (HealthBenefitId == "114")
                                        {
                                            //DELTACARE PLAN D
                                            sbcId = "83";
                                        }
                                        break;
                                    case 3:
                                        //if (HealthBenefitId == "1")
                                        //{
                                        //    sbcId = "42";
                                        //}

                                        break;
                                    case 4:
                                        //Prescription Coverage
                                        //kAISER PLAN A MRV
                                        if (HealthBenefitId == "116")
                                        {
                                            //Kaiser Rx Coverage
                                            sbcId = "84";
                                        }
                                        //KAISER PLAN B MRV 
                                        else if (HealthBenefitId == "119")
                                        {
                                            //Kaiser Rx Coverage
                                            sbcId = "85";
                                        }
                                        //KAISER PLAN C MR
                                        else if (HealthBenefitId == "122")
                                        {
                                            //Kaiser Rx Coverage
                                            sbcId = "86";
                                        }
                                        //KAISER PLAN D MR
                                        else if (HealthBenefitId == "124")
                                        {
                                            //Kaiser Rx Coverage
                                            sbcId = "87";
                                        }
                                        //OPTUM PLAN A RX 
                                        else if (HealthBenefitId == "126")
                                        {
                                            //OPTUM RX
                                            sbcId = "88";
                                        }
                                        //OPTUM PLAN B RX 
                                        else if (HealthBenefitId == "127")
                                        {
                                            //OPTUM RX
                                            sbcId = "89";
                                        }
                                        //OPTUM PLAN C RX 
                                        else if (HealthBenefitId == "128")
                                        {
                                            //OPTUM RX
                                            sbcId = "90";
                                        }
                                        //OPTUM PLAN D RX 
                                        else if (HealthBenefitId == "129")
                                        {
                                            //OPTUM RX
                                            sbcId = "91";
                                        }
                                       
                                        break;

                                    case 5:
                                        //AETNA PLAN A Vision 
                                        if (HealthBenefitId == "98")
                                        {
                                            //AETNA PLAN A 
                                            sbcId = "92";
                                        }
                                        //AETNA PLAN B Vision 
                                        else if (HealthBenefitId == "100")
                                        {
                                            //AETNA PLAN B 
                                            sbcId = "93";
                                        }
                                        //ANTHEM PLAN A Vision 
                                        else if (HealthBenefitId == "104")
                                        {
                                            //ANTHEM PLAN A Vision 
                                            sbcId = "94";
                                        }
                                        //ANTHEM PLAN B Vision 
                                        else if (HealthBenefitId == "106")
                                        {
                                            //ANTHEM PLAN B Vision 
                                            sbcId = "95";
                                        }
                                        //KAISER PLAN A MRV
                                        else if (HealthBenefitId == "115")
                                        {
                                            //KAISER PLAN A MRV
                                            sbcId = "96";
                                        }
                                        //KAISER PLAN B MRV
                                        else if (HealthBenefitId == "118")
                                        {
                                            //KAISER PLAN B MRV
                                            sbcId = "97";
                                        }

                                        //HMO Plan B Vision
                                        else if (HealthBenefitId == "130")
                                        {
                                            //ANTHEM PLAN B Vision 
                                            sbcId = "98";
                                        }
                                        //HMO Plan A Vision
                                        else if (HealthBenefitId == "131")
                                        {
                                            //ANTHEM PLAN A Vision 
                                            sbcId = "99";
                                        }
                                        //PAD PLAN A Vision 
                                        else if (HealthBenefitId == "132")
                                        {
                                            //ANTHEM PLAN A Vision 
                                            sbcId = "100";
                                        }
                                        //PAD PLAN B Vision 
                                        else if (HealthBenefitId == "133")
                                        {
                                            //ANTHEM PLAN A Vision 
                                            sbcId = "101";
                                        }

                                        break;
                                  
                                }
                                if (EligKey == "" || sbcId == "")
                                {

                                    Console.WriteLine("could not link record");
                                    continue;
                                }
                                dupelig = coverageTypeCode + "|" + eligMonth + '|' + healthBenefitCode + '|' + EligKey + "|" + sbcId;
                                if (dups_elig.ContainsKey(dupelig))
                                {
                                    Console.WriteLine("Duplicate Elig Record:" + extractL[0] + "|" + extractL[3] + "|" + coverageTypeCode + "|" + eligMonth + '|' + healthBenefitCode + '|' + EligKey + "|" + sbcId);
                                    continue;
                                }
                                else
                                {
                                    dups_elig.Add(coverageTypeCode + "|" + eligMonth + '|' + healthBenefitCode + '|' + EligKey + "|" + sbcId, 1);

                                }
                                elig = e + "|" + coverageTypeCode + "|" + eligMonth + '|' + healthBenefitCode + '|' + EligKey + "|" + sbcId;
                                e++;
                                elines.Add(elig);

                            }
                            else
                            {
                                continue;
                            }

                        }


                    }
                }
                //using (new Impersonator("sqladmin", "selarom", "5Trgw7B@56Z"))
                //{

                //    System.IO.File.WriteAllLines(@"\\lvshare01\\Company\\Share\\PSWA\\Loads\\LB\\Outgoing\\Eligibility\\Elig_LB.csv", elines.ToArray());
                //}
                System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\Outgoing\\Elig_QR.csv", elines.ToArray());
                System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\Elig_Dups_QR.csv", dupeliglines.ToArray());
                System.IO.File.WriteAllLines("C:\\PSWA\\Loads\\QR\\Eligibility\\Error_Log_QR.csv", error_log.ToArray());

                File.Delete(@"C:\PSWA\Loads\QR\Eligibility\Incoming\" + FileName);



                System.Console.WriteLine("Total Dups = ", x);
    

                EmailTitle = "Painting and Drywall eligibility load";
                EmailSubTitle = "First Phase";
                EmailBody = "This is an automated notification. The first phase has created the necessary files. Loaded from file name " + FileName;
                recipients = "mmorales@selarom.us|support@pswadmin.com";
                string result = await APIRequest(EmailTitle, EmailSubTitle, EmailBody, recipients);


            }
            else
            {
                EmailTitle = "Painting and Drywall eligibility load";
                EmailSubTitle = "First Phase";
                EmailBody = "This is an automated notification. The first phase has Failed because of an incorrect file name.";
                recipients = "mmorales@selarom.us|support@pswadmin.com";
                await APIRequest(EmailTitle, EmailSubTitle, EmailBody, recipients);
                Environment.Exit(0);
            }
        }


        public static async Task<string> APIRequest(string EmailTitle, string EmailSubTitle, string EmailBody, string recipients)
        {
            HttpClient clientElig = new HttpClient();
            clientElig.BaseAddress = new Uri("http://10.5.1.64:5000/api/eligibility_lb/EmailService");
            clientElig.DefaultRequestHeaders.Accept.Clear();
            clientElig.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage responseElig = await clientElig.GetAsync("?EmailTitle=" + EmailTitle + "&EmailSubTitle=" + EmailSubTitle + "&EmailBody=" + EmailBody + "&recipients=" + recipients);

            return "Successfull";
        }

    }
    
}