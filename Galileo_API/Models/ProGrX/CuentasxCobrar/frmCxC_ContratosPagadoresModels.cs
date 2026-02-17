using System;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxcContratoPagadorDto
    {
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public DateTime? Fecha { get; set; }
        public int Activo { get; set; }
    }

    public class CxcContratoPagadorListaParams
    {
        public string Cod_Contrato { get; set; }
        public required bool ChkTodos { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
    }

    public class CxcContratoPagadorSaveParams
    {
        public string Cod_Contrato { get; set; }
        public string Cedula { get; set; }
        public string Registro_Usuario { get; set; }
    }

    public class CxcContratoPagadorDeleteParams
    {
        public string Cod_Contrato { get; set; }
        public string Cedula { get; set; }
    }
}
