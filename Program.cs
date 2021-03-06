using System;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

// "Transaction Date","Posting Date","Billing Amount","Merchant","Merchant City","Merchant County","Merchant Postal Code","Reference Number","Debit/Credit Flag","SICMCC Code"
// "2021-05-14","2021-05-14",£0.99,"GOOGLE Google Play Ap","London","GBR","EC4A 3TW","15265671134000327634031","Debit","5816"

namespace TescoCsvConv
{
    enum TxnType
    {
        Credit,
        Debit, 
    }

    class TescoEntry
    {
        public string TxnDate { get; set; }
        public string PostingDate { get; set; }
        public string Amount { get; set; }
        public string Merchant { get; set; }
        public string MerchantCity { get; set; }
        public string MerchantCountry { get; set; }
        public string MerchantPostalCode { get; set; }
        public string RefNumber { get; set; }
        public TxnType Type { get; set; }
        public string Code { get; set; }
    }

    sealed class TescoEntryMap : ClassMap<TescoEntry>
    {
        // "Transaction Date","Posting Date","Billing Amount","Merchant","Merchant City",
        // "Merchant County","Merchant Postal Code","Reference Number","Debit/Credit Flag","SICMCC Code"
            
        public TescoEntryMap()
        {
            Map(m => m.TxnDate).Name("Transaction Date");
            Map(m => m.PostingDate).Name("Posting Date");
            Map(m => m.Amount).Name("Billing Amount");
            Map(m => m.Merchant).Name("Merchant");
            Map(m => m.MerchantCity).Name("Merchant City");
            Map(m => m.MerchantCountry).Name("Merchant County");
            Map(m => m.MerchantPostalCode).Name("Merchant Postal Code");
            Map(m => m.RefNumber).Name("Reference Number");
            Map(m => m.Type).Name("Debit/Credit Flag");
            Map(m => m.Code).Name("SICMCC Code");
        }
    }

    class YnabTxn
    {
        public string Date { get; set; }
        public string Payee { get; set; }
        public string Memo { get; set; }
        public string Outflow { get; set; }
        public string Inflow { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: TescoCsvConv <input.csv> <output.csv>");
                return;
            }
            
            var csvConfig = new CsvConfiguration(CultureInfo.GetCultureInfo("en-gb"));
            csvConfig.HasHeaderRecord = true;
            csvConfig.BadDataFound = context => Console.WriteLine("Bad row: ${0}", context.RawRecord);

            // ISO-8859-1 encoding - Id 28591
            using (var csvReader = new CsvReader(new StreamReader(args[0], Encoding.GetEncoding(28591)), csvConfig))
            using (var outStream = File.Open(args[1], FileMode.Create))
            using (var writer = new StreamWriter(outStream, new System.Text.UTF8Encoding(true)))
            using (var csvWriter = new CsvWriter(writer, csvConfig))
            {
                csvReader.Context.RegisterClassMap<TescoEntryMap>();
                var entries = csvReader.GetRecords<TescoEntry>();

                csvWriter.WriteHeader<YnabTxn>();
                csvWriter.NextRecord();

                foreach (var entry in entries)
                {
                    YnabTxn t = new YnabTxn();
                    t.Date = entry.TxnDate;
                    t.Payee = entry.Merchant;
                    switch (entry.Type)
                    {
                        case TxnType.Credit:
                            t.Inflow = entry.Amount.TrimStart('-');
                            break;
                        case TxnType.Debit:
                            t.Outflow = entry.Amount;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    csvWriter.WriteRecord(t);
                    csvWriter.NextRecord();
                }
            }
        }
    }
}
