using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCConsultaBL
    {

        private readonly FrmCxCConsultaDB _db;

        public FrmCxCConsultaBL(IConfiguration config) => _db = new FrmCxCConsultaDB(config);

        /// <summary>Consulta una persona por cédula o número de operación.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula o número de operación.</param>
        /// <returns>Persona encontrada o null.</returns>
        public ErrorDto<CxCPersonaDto?> ConsultarPersona(int codEmpresa, string cedula)
        {
            return _db.ConsultarPersona(codEmpresa, cedula);
        }

        /// <summary>Obtiene las personas de CxC para la búsqueda F4.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="filtro">Texto para filtrar por cédula o nombre.</param>
        /// <returns>Personas disponibles ordenadas por nombre.</returns>
        public ErrorDto<List<CxCPersonaDto>> ConsultarPersonasF4(int codEmpresa, string? filtro)
        {
            return _db.ConsultarPersonasF4(codEmpresa, filtro);
        }

        /// <summary>Consulta las cuentas de una persona por estado.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="estado">Estado de las operaciones.</param>
        /// <returns>Cuentas asociadas.</returns>
        public ErrorDto<List<CxCCuentaDto>> ConsultarCuentas(int codEmpresa, string cedula, string estado)
        {
            return _db.ConsultarCuentas(codEmpresa, cedula, estado);
        }

        /// <summary>Consulta las solicitudes de una persona.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Solicitudes asociadas.</returns>
        public ErrorDto<List<CxCSolicitudDto>> ConsultarSolicitudes(int codEmpresa, string cedula)
        {
            return _db.ConsultarSolicitudes(codEmpresa, cedula);
        }

        /// <summary>Consulta los preanálisis de una persona.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Preanálisis asociados.</returns>
        public ErrorDto<List<CxCPreAnalisisDto>> ConsultarPreAnalisis(int codEmpresa, string cedula)
        {
            return _db.ConsultarPreAnalisis(codEmpresa, cedula);
        }

        /// <summary>Consulta las operaciones incobrables de una persona.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Operaciones incobrables asociadas.</returns>
        public ErrorDto<List<CxCIncobrableDto>> ConsultarIncobrables(int codEmpresa, string cedula)
        {
            return _db.ConsultarIncobrables(codEmpresa, cedula);
        }

        /// <summary>Consulta las facturas según los filtros de pantalla.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="filtro">Filtros de la consulta.</param>
        /// <returns>Facturas encontradas.</returns>
        public ErrorDto<List<CxCFacturaDto>> ConsultarFacturas(int codEmpresa, CxCFacturaFiltroDto filtro)
        {
            return _db.ConsultarFacturas(codEmpresa, filtro);
        }

        /// <summary>Consulta los desembolsos de una persona.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Desembolsos asociados.</returns>
        public ErrorDto<List<CxCDesembolsoDto>> ConsultarDesembolsos(int codEmpresa, string cedula)
        {
            return _db.ConsultarDesembolsos(codEmpresa, cedula);
        }

        /// <summary>Consulta los mensajes vigentes de una persona.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Mensajes vigentes.</returns>
        public ErrorDto<List<CxCMensajeDto>> ConsultarMensajes(int codEmpresa, string cedula)
        {
            return _db.ConsultarMensajes(codEmpresa, cedula);
        }

        /// <summary>Registra un mensaje para una persona.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="dto">Datos del mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> GuardarMensaje(int codEmpresa, CxCMensajeAddDto dto)
        {
            return _db.GuardarMensaje(codEmpresa, dto);
        }

        /// <summary>Elimina un mensaje registrado.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="dto">Llave del mensaje.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> EliminarMensaje(int codEmpresa, CxCMensajeDeleteDto dto)
        {
            return _db.EliminarMensaje(codEmpresa, dto);
        }

        /// <summary>Consulta las facturas relacionadas con un giro.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <param name="idGiro">Identificador del giro.</param>
        /// <returns>Facturas asociadas al giro.</returns>
        public ErrorDto<List<CxCDesembolsoFacturaDto>> ConsultarFacturasPorGiro(int codEmpresa, int operacion, int idGiro)
        {
            return _db.ConsultarFacturasPorGiro(codEmpresa, operacion, idGiro);
        }

        /// <summary>Consulta los estados disponibles para las facturas.</summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Estados configurados en la empresa.</returns>
        public ErrorDto<List<CxCFacturaEstadoDto>> ConsultarEstadosFactura(int codEmpresa)
        {
            return _db.ConsultarEstadosFactura(codEmpresa);
        }
    }



}

