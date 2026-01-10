namespace Galileo_API.Models.ProGrX.Cajas
{
    // Clase base con las propiedades comunes
    public class CajasDefinicionBase
    {
        public short Activa { get; set; }
        public DateTime Apertura_Fecha { get; set; }
        public short Periocidad_Contrasena { get; set; }
        public short Oficina_Utiliza_Usuario { get; set; }
    }

    public class CajasDefinicionDetalleModel : CajasDefinicionBase
    {
        public string? Cod_Caja { get; set; }
        public string? Descripcion { get; set; }
        public string? Notas { get; set; }
        public int? Apertura_Codigo { get; set; }
        public short? Apertura_Compartida { get; set; }
        public string? Cierre_Periocidad { get; set; }
        public string? Cierre_Tipo { get; set; }
        public string? Cod_Oficina { get; set; }
        public string? Cod_Cuenta_Dev { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public short? Permite_Mov_Cbrjud { get; set; }
        public short? Limita_Consulta { get; set; }
        public short? Limita_Creditos { get; set; }
        public short? Limita_Fondos { get; set; }
        public short? Limita_Cxc { get; set; }
        public short? Limita_Patrimonio { get; set; }
        public short? Permite_Rc { get; set; }
        public short? Permite_Traslados_Ef { get; set; }
        public short? Rol_Boveda { get; set; }
        public short? Utiliza_Cta_Caja_Ef { get; set; }
        public short? Limita_Fondos_Fp { get; set; }
        public string? OficinaDesc { get; set; }
        public string? CuentaDesc { get; set; }
    }

    public class CajasDivisaPoliticaModel
    {
        public string? Cod_Divisa { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Efec_Max { get; set; }
        public decimal? Efec_Min { get; set; }
        public decimal? Doc_Max { get; set; }
        public decimal? Doc_Min { get; set; }
    }

    public class CajasRecaudadorModel
    {
        public string? Cod_Recaudador { get; set; }
        public string? Descripcion { get; set; }
    }

    public class CajasServicioAsignadoModel
    {
        public string? Cod_Servicio { get; set; }
        public string? Descripcion { get; set; }
        public string? Asignado { get; set; }
    }

    public class CajasServicioAsignarParams
    {
        public string? Cod_Caja { get; set; }
        public string? Cod_Servicio { get; set; }
        public string? Cod_Recaudador { get; set; }
        public string? Usuario { get; set; }
    }

    public class CajasAuxiliarAsignadoModel
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public string? Asignado { get; set; }
    }

    public class CajasAuxiliarFiltroParams
    {
        public string? CodCaja { get; set; }
        public string? AuxFiltro { get; set; }
    }

    public class CajasAuxiliarAsignarParams
    {
        public string? Tipo { get; set; }
        public string? CodAuxiliar { get; set; }
        public string? CodCaja { get; set; }
        public string? Usuario { get; set; }
    }

    public class CajasFormaPagoAsignadoModel
    {
        public string? Cod_Forma_Pago { get; set; }
        public string? Descripcion { get; set; }
        public string? Asignado { get; set; }
    }

    public class CajasFormaPagoAsignarParams
    {
        public string? Cod_Forma_Pago { get; set; }
        public string? Cod_Caja { get; set; }
        public string? Usuario { get; set; }
    }

    public class CajasDocumentoAsignadoModel
    {
        public string? Tipo_Documento { get; set; }
        public string? Descripcion { get; set; }
        public string? Asignado { get; set; }
    }

    public class CajasDocumentoAsignarParams
    {
        public string? Tipo_Documento { get; set; }
        public string? Cod_Caja { get; set; }
        public string? Usuario { get; set; }
    }

    public class CajasUsuarioHistorialModel
    {
        public string? Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Salida_Fecha { get; set; }
        public string? Salida_Usuario { get; set; }
    }

    public class CajasDefinicionInsertParams : CajasDefinicionBase
    {
        public required string Cod_Caja { get; set; }
        public string? Descripcion { get; set; }
        public string? Notas { get; set; }
        public short? Apertura_Compartida { get; set; }
        public string? Cierre_Periocidad { get; set; }
        public string? Cierre_Tipo { get; set; }
        public string? Cod_Oficina { get; set; }
        public string? Cod_Cuenta_Dev { get; set; }
        public short? Permite_Mov_Cbrjud { get; set; }
        public short? Limita_Consulta { get; set; }
        public short? Limita_Creditos { get; set; }
        public short? Limita_Fondos { get; set; }
        public short? Limita_Cxc { get; set; }
        public short? Limita_Patrimonio { get; set; }
        public short? Permite_Rc { get; set; }
        public short? Permite_Traslados_Ef { get; set; }
        public short? Rol_Boveda { get; set; }
        public short? Utiliza_Cta_Caja_Ef { get; set; }
        public short? Limita_Fondos_Fp { get; set; }
        public required string Registro_Usuario { get; set; }
    }

    public class CajasDefinicionCopiaParams
    {
        public required string CajaOrigen { get; set; }
        public required string CajaDestino { get; set; }
        public required string Usuario { get; set; }
        public required string CajaNombre { get; set; }
    }

}
