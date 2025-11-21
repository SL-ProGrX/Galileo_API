namespace Galileo.Models.ERROR
{
    public class ErrorDto<T>
    {
        public T? Result { get; set; }
        public int? Code { get; set; }
        public string? Description { get; set; }
    }

    public class ErrorDto
    {
        public int? Code { get; set; }
        public string? Description { get; set; } = string.Empty;
    }

}
