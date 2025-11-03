namespace PgxAPI.Models.ERROR
{
    public class ErrorDTO<T>
    {
        public T? Result { get; set; }
        public int? Code { get; set; }
        public string? Description { get; set; }
    }

    public class ErrorDTO
    {
        public int? Code { get; set; }
        public string? Description { get; set; } = string.Empty;
    }

}
