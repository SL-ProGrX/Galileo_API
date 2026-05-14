namespace Galileo.Models.ProGrX.Fondos
{
    public class FndBancosXModel
    {
        public int Id_Banco { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Cheque { get; set; }
        public bool Transferencia { get; set; }
    }

    public class FndBancosXUpdateParam
    {
        public required int IdBanco { get; set; }
        public string Campo { get; set; } = string.Empty; // "cheque" o "transferencia"
        public required bool Valor { get; set; }
    }
}
