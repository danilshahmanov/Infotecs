namespace Infotecs.Source.Data.DTOs
{
    //Используется для получения определенной информации сущности Value из базы данных
    public class ValueDto
    {
        public DateTime StartDateTime { get; set; }
        public int Duration { get; set; }
        public float IndicatorValue { get; set; }
    }
}
