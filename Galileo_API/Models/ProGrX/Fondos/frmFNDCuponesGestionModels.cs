namespace Galileo.Models.ProGrX.Fondos
{
    public class FndCuponesGestionPlanExisteResult
    {
        public int Existe { get; set; }
    }

    public class FndCuponesGestionVencimientoResult
    {
        public int Cod_Operadora { get; set; }
        public string? Cod_Plan { get; set; }
        public int Cod_Contrato { get; set; }
        public int Consec { get; set; }
        public DateTime Fecha_Vence { get; set; }
        public decimal Principal { get; set; }
        public decimal Rendimiento { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public decimal Total { get; set; }
        public int DiasTransc { get; set; }
        public int Id_Banco { get; set; }
        public string? BancoDesc { get; set; }
        public string? Tipo_Pago { get; set; }
        public string? Cuenta_Ahorros { get; set; }
        public decimal ISR { get; set; }
        public decimal Neto { get; set; }
    }

    public class FndCuponesGestionVencimientoParams
    {
        public required int CodEmpresa { get; set; }
        public required DateTime FechaInicio { get; set; }
        public required DateTime FechaCorte { get; set; }
        public required int CodOperadora { get; set; }
        public string CodPlan { get; set; } = "";
        public string Proceso { get; set; } = "D"; // 'D' o 'R'
        public string TipoPago { get; set; } = "TE"; // "TE","CK","RC","FD","RT"
        public int BancoId { get; set; } = 0;
        public bool chkFechas { get; set; } = false;
    }

    public class FndCuponesGestionLiquidaParams
    {
        public required int CodEmpresa { get; set; }
        public required int CodOperadora { get; set; }
        public string? CodPlan { get; set; }
        public required int Contrato { get; set; }
        public required int CuponId { get; set; }
        public string? Usuario { get; set; }
        public string? Proceso { get; set; } // 'D' o 'R'
        public string RetencionCodigo { get; set; } = "";
        public string TipoDoc { get; set; } = "OT"; // "TE","CK","RC","FD", etc.
        public int BancoId { get; set; } = 0;
        public string CuentaPersona { get; set; } = "";
        public int TesoreriaFlag { get; set; } = 0;
        public string Descripcion { get; set; } = "";
        public string AppName { get; set; } = "ProGrX";
    }

    public class FndCuponesGestionLiquidaResult
    {
        public int Liq_Num { get; set; }
        public decimal MontoGiro { get; set; }
        public int Tesoreria { get; set; }
        public int Num_Liq { get; set; }
    }
}
