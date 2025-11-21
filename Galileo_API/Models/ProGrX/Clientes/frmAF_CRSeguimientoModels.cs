namespace Galileo.Models.ProGrX.Clientes
{
    public class AfCrSeguimientoData
    {
        public int CodRenuncia { get; set; }
        public string? EstadoDesc { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? TipoRenuncia { get; set; }
        public DateTime? Vencimiento { get; set; }
        public string? CausaDesc { get; set; }
        public string? EjecutivoDesc { get; set; }
        public string? RegistroUser { get; set; }
        public DateTime? RegistroFecha { get; set; }
        public string? ResueltoUser { get; set; }
        public DateTime? ResueltoFecha { get; set; }
    }

    public class AfCrSeguimientoFiltros
    {
        public int? RenunciaIni { get; set; }
        public int? RenunciaFin { get; set; }
        public string? Estado { get; set; }
        public string? TipoChar { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Usuario { get; set; }
        public string? Ejecutivo { get; set; }
        public int? IdCausa { get; set; }
        public int? IdInstitucion { get; set; }
        public string? Provincia { get; set; }
        public string? Zona { get; set; }
        public string? UsuarioActual { get; set; }
        public string? TipoFecha { get; set; }
        public DateTime? FIni { get; set; }
        public DateTime? FFin { get; set; }
        public bool AplicarChecks { get; set; }
        public byte? Mortalidad { get; set; }
        public byte? Reingreso { get; set; }
        public byte? Volver { get; set; }
        public byte? AumentoTasas { get; set; }
    }

    public class AfCrSeguimientoDetalle
    {
        public int Cod_Renuncia { get; set; }
        public string? Estado { get; set; }
        public string? Estado_Desc { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Tipo { get; set; }
        public DateTime? Vencimiento { get; set; }
        public int? Id_Causa { get; set; }
        public string? CausaX { get; set; }
        public string? Causa_Desc { get; set; }
        public string? Ejecutivo_Desc { get; set; }
        public string? Registro_User { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Resuelto_User { get; set; }
        public DateTime? Resuelto_Fecha { get; set; }
        public int? Id_Promotor { get; set; }
        public string? PromotorX { get; set; }
        public string? Notas { get; set; }
        public int Aplica_Reingreso { get; set; }
        public int Mortalidad { get; set; }
        public int Volver { get; set; }
    }
    
    public class AfCrSeguimientoMotivo
    {
        public string Cod_Motivo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Asignado { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_User { get; set; }
    }

    public class AfCrSeguimientoHistorial
    {
        public int ID { get; set; }
        public string Cod_Renuncia { get; set; } = "";
        public string Cod_Gestion { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime? Fecha { get; set; }
        public string? Usuario { get; set; }
        public string? Notas { get; set; }
    }

    public class AfCrSeguimientoMotivosRegistrar
    {
        public int RenunciaId { get; set; }
        public string Motivo { get; set; } = "";
        public string TipoMov { get; set; } = "";
        public string? Usuario { get; set; }
    }

    public class AfCrSeguimientoRenunciaEstado
    {
        public int RenunciaId { get; set; }
        public string Estado { get; set; } = "";
        public string Gestion { get; set; } = "";
        public string? Notas { get; set; } = "";
        public string? Usuario { get; set; }
        public string? Equipo { get; set; }
        public string? Version { get; set; }
    }
}