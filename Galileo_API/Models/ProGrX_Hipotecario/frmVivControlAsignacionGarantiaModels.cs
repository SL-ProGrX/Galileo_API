namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivControlAsignacionGarantiaPendienteData
    {
        public int idGarantia { get; set; } = 0;
        public int numeroOperacion { get; set; } = 0;
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string numeroFinca { get; set; } = string.Empty;
        public string numPlanoCatastro { get; set; } = string.Empty;
        public string tipoDerecho { get; set; } = string.Empty;
        public string descGradoHipoteca { get; set; } = string.Empty;
        public decimal areaFinca { get; set; } = 0;
        public string descZona { get; set; } = string.Empty;
        public string registroUsuario { get; set; } = string.Empty;
        public DateTime? registroFecha { get; set; }
        public int idZona { get; set; } = 0;
    }

    public class VivControlAsignacionProfesionalData
    {
        public int idZona { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public int idGarantia { get; set; } = 0;
        public string identificacion { get; set; } = string.Empty;
        public int idEmpresa { get; set; } = 0;
        public string tipoProfesional { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string profesional { get; set; } = string.Empty;
        public string nombreEmpresa { get; set; } = string.Empty;
        public int cantOp { get; set; } = 0;
        public decimal montoapr { get; set; } = 0;
        public int itemAsignado { get; set; } = 0;
        public string condicion { get; set; } = "N";
    }

    public class VivControlAsignacionGarantiaAsignarRequest
    {
        public int idGarantia { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public string tipoProfesional { get; set; } = string.Empty;
        public DateTime? fecha_asignacion { get; set; }
    }

    public class VivControlEntregaGarantiaData
    {
        public int idGarantia { get; set; } = 0;
        public int numeroOperacion { get; set; } = 0;
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string numeroFinca { get; set; } = string.Empty;
        public string numPlanoCatastro { get; set; } = string.Empty;
        public string tipoDerecho { get; set; } = string.Empty;
        public string descGradoHipoteca { get; set; } = string.Empty;
        public decimal areaFinca { get; set; } = 0;
        public string descZona { get; set; } = string.Empty;
        public string asignacionUsuario { get; set; } = string.Empty;
        public DateTime? asignacionFecha { get; set; }
        public int idZona { get; set; } = 0;
        public string condicion { get; set; } = "N";
        public int diasTransProfesional { get; set; } = 0;
        public DateTime? entregaFecha { get; set; }
    }

    public class VivControlEntregaGarantiaRequest
    {
        public int idGarantia { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public string tipoProfesional { get; set; } = string.Empty;
        public string aplicar { get; set; } = string.Empty;
    }

    public class VivControlAsignacionGarantiaNotaData
    {
        public string ultima_nota { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string fecha_registro { get; set; } = string.Empty;
        public string numero_operacion { get; set; } = string.Empty;
        public string numeroFinca { get; set; } = string.Empty;
    }

    public class VivControlRecibeGarantiaData
    {
        public int idGarantia { get; set; } = 0;
        public int numeroOperacion { get; set; } = 0;
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string numeroFinca { get; set; } = string.Empty;
        public string numPlanoCatastro { get; set; } = string.Empty;
        public string tipoDerecho { get; set; } = string.Empty;
        public string descGradoHipoteca { get; set; } = string.Empty;
        public decimal areaFinca { get; set; } = 0;
        public string descZona { get; set; } = string.Empty;
        public string entregaUsuario { get; set; } = string.Empty;
        public DateTime? entregaFecha { get; set; }
        public int idZona { get; set; } = 0;
        public string condicion { get; set; } = "N";
        public int diasTransProfesional { get; set; } = 0;
        public DateTime? recepcionFecha { get; set; }
        public DateTime? firmasFecha { get; set; }
    }

    public class VivControlRecibeGarantiaRequest
    {
        public int idGarantia { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public string tipoProfesional { get; set; } = string.Empty;
        public string aplicar { get; set; } = string.Empty;
    }

    public class VivControlRegistroGarantiaData
    {
        public int idGarantia { get; set; } = 0;
        public int numeroOperacion { get; set; } = 0;
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string numeroFinca { get; set; } = string.Empty;
        public string numPlanoCatastro { get; set; } = string.Empty;
        public string tipoDerecho { get; set; } = string.Empty;
        public string descGradoHipoteca { get; set; } = string.Empty;
        public decimal areaFinca { get; set; } = 0;
        public string descZona { get; set; } = string.Empty;
        public string asignacionUsuario { get; set; } = string.Empty;
        public DateTime? asignacionFecha { get; set; }
        public int idZona { get; set; } = 0;
        public int diasTransProfesional { get; set; } = 0;
        public int diasTransAbogado { get; set; } = 0;
        public DateTime? firmasFecha { get; set; }
    }

    public class VivControlRegistroGarantiaRequest
    {
        public int idGarantia { get; set; } = 0;
        public int idContacto { get; set; } = 0;
        public string tipoProfesional { get; set; } = string.Empty;
        public string aplicar { get; set; } = string.Empty;
    }
}