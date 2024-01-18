using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Infotecs.Source.Data.Models;

namespace Infotecs.Source.Data.Mappings
{
    //используется для парсинга float где вместо точки - запятая
    public class CommaFloatTypeConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(
            string? text,
            IReaderRow row,
            MemberMapData memberMapData
        )
        {
            if (string.IsNullOrWhiteSpace(text))
                return default(float);
            var processedText = text.Replace(',', '.');
            if (
                float.TryParse(
                    processedText,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out float result
                )
            )
                return result;
            return base.ConvertFromString(text, row, memberMapData);
        }
    }

    //используется для мапинга сущности Value из csv файла
    public sealed class ValueMap : ClassMap<Value>
    {
        public ValueMap()
        {
            Map(m => m.StartDateTime).Index(0).TypeConverterOption.Format("yyyy-MM-dd_HH-mm-ss");
            Map(m => m.Duration).Index(1);
            Map(m => m.IndicatorValue).Index(2).TypeConverter<CommaFloatTypeConverter>();
            ;
        }
    }
};
