using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security; 
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCConsultaDB
    {
        private readonly PortalDB _portalDB;

        public FrmCxCConsultaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Consulta Persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CxCPersonaDto?> ConsultarPersona(int codEmpresa, string cedula)
        {
            var response = new ErrorDto<CxCPersonaDto?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.QueryFirstOrDefault<CxCPersonaDto>(
                    @"SELECT 
                        P.cedula,
                        P.nombre,
                        C.descripcion AS categoria_desc,
                        dbo.fxCxC_PersonasMsjNum(P.cedula) AS ind_mensajes,
                        ISNULL(Fa.Facturas,0) AS facturas,
                        ISNULL(Fa.Total,0) AS facturas_total
                      FROM CxC_Personas P
                      INNER JOIN CxC_Categoria_Clientes C 
                        ON P.cod_categoria = C.cod_categoria
                      LEFT JOIN vCxC_C_Persona_Facturas_Anio Fa 
                        ON P.cedula = Fa.cedula
                      WHERE P.cedula = @cedula",
                    new { cedula }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta cuentas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCCuentaDto>> ConsultarCuentas(int codEmpresa, string cedula, int tipo)
        {
            var response = new ErrorDto<List<CxCCuentaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CxCCuentaDto>(
                    "spCxC_PersonasCuentas",
                    new { cedula, tipo },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Consulta solicitudes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCSolicitudDto>> ConsultarSolicitudes(int codEmpresa, string cedula)
            => EjecutarSP<List<CxCSolicitudDto>>(codEmpresa, "spSIFEstadoSolicitud", new { cedula });

        /// <summary>
        /// Consulta Preanalisis
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCPreAnalisisDto>> ConsultarPreAnalisis(int codEmpresa, string cedula)
            => EjecutarSP<List<CxCPreAnalisisDto>>(codEmpresa, "spSIFEstadoPreAnalisis", new { cedula });

        /// <summary>
        /// Consulta incobrables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCIncobrableDto>> ConsultarIncobrables(int codEmpresa, string cedula)
            => EjecutarSP<List<CxCIncobrableDto>>(codEmpresa, "spSIFEstadoIncobrable", new { cedula });

        /// <summary>
        /// Consulta deseembolsos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCDesembolsoDto>> ConsultarDesembolsos(int codEmpresa, string cedula)
        {
            var response = new ErrorDto<List<CxCDesembolsoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CxCDesembolsoDto>(
                    @"SELECT Ct.OPERACION, Cg.MONTO, Td.ESTADO, Td.FECHA_EMISION,
                             Td.TIPO, Cg.Id_Giro, Tb.DESCRIPCION AS banco_desc,
                             Td.BENEFICIARIO, Td.NDOCUMENTO
                      FROM CXC_CUENTAS Ct
                      INNER JOIN CXC_CUENTAS_GIROS Cg ON Ct.OPERACION = Cg.OPERACION
                      INNER JOIN TES_TRANSACCIONES Td ON Cg.TESORERIA_SOLICITUD = Td.NSOLICITUD
                      INNER JOIN TES_BANCOS Tb ON Td.ID_BANCO = Tb.ID_BANCO
                      WHERE Ct.CEDULA = @cedula",
                    new { cedula }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta mensajes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCMensajeDto>> ConsultarMensajes(int codEmpresa, string cedula)
            => EjecutarSP<List<CxCMensajeDto>>(codEmpresa, "CxC_Personas_Mensajes_Consulta", new { cedula });

        /// <summary>
        /// Guarda mensajes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> GuardarMensaje(int codEmpresa, CxCMensajeAddDto dto)
            => EjecutarNonQuery(codEmpresa, "CxC_Personas_Mensajes_Insert", dto);

        /// <summary>
        /// Elimina mensajes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<bool> EliminarMensaje(int codEmpresa, CxCMensajeDeleteDto dto)
            => EjecutarNonQuery(codEmpresa, "CxC_Personas_Mensajes_Delete", dto);

        /// <summary>
        /// Ejecuta SP
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sp"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private ErrorDto<T> EjecutarSP<T>(int codEmpresa, string sp, object param)
        {
            var response = new ErrorDto<T>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<T>(
                    sp,
                    param,
                    commandType: System.Data.CommandType.StoredProcedure
                ).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Ejecuta NonQuery
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sp"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private ErrorDto<bool> EjecutarNonQuery(int codEmpresa, string sp, object param)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(sp, param, commandType: System.Data.CommandType.StoredProcedure);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    
        /// <summary>
        /// Consulta Facturas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
         public ErrorDto<List<CxCFacturaDto>> ConsultarFacturas(int codEmpresa,CxCFacturaFiltroDto filtro)
        {
            var response = new ErrorDto<List<CxCFacturaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                Operacion        AS operacion,
                cod_Factura      AS cod_factura,
                Fecha_Pago       AS fecha_pago,
                Monto            AS monto,
                Adelanto_Monto   AS adelanto_monto,
                Liberado         AS liberado,
                Pagador_Nombre   AS pagador_nombre,
                Factura_Estado_Desc AS factura_estado_desc
            FROM vCxC_Facturas_Control
            WHERE cedula = @cedula
              AND cod_Factura LIKE '%' + @cod_factura + '%'
        ";

                if (filtro.operacion.HasValue)
                {
                    sql += " AND operacion = @operacion";
                }

                switch (filtro.tipo_fecha)
                {
                    case "Registro":
                        sql += " AND Registro_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Emisión":
                        sql += " AND fecha_Emision BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Pago":
                        sql += " AND fecha_Pago BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Libera":
                        sql += " AND Liberado_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Cancela":
                        sql += " AND Cancela_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Activación":
                        sql += " AND Activa_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Desembolso 1":
                        sql += " AND Pago_Principal_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;

                    case "Desembolso 2":
                        sql += " AND Pago_Secundario_Fecha BETWEEN @fecha_inicio AND @fecha_corte";
                        break;
                }

                if (!string.IsNullOrEmpty(filtro.estado) && filtro.estado != "TODOS")
                {
                    sql += " AND factura_estado = @estado";
                }

                response.Result = cn.Query<CxCFacturaDto>(
                    sql,
                    new
                    {
                        filtro.cedula,
                        filtro.cod_factura,
                        filtro.operacion,
                        filtro.fecha_inicio,
                        filtro.fecha_corte,
                        filtro.estado
                    }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}






