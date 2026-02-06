using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysMonitorAfiliacionEnLineaDB
    {
        private readonly PortalDB _portalDB;

        public FrmSysMonitorAfiliacionEnLineaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }
        public ErrorDto<List<AfiliacionTablaDto>> Buscar(int codEmpresa,AfiliacionFiltroDto filtros)
        {
            var response = new ErrorDto<List<AfiliacionTablaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                        SELECT 
                            SOLICITUD_ID,
                            Estado_Desc,
                            CEDULA,
                            ID_COLILLA,
                            APELLIDO_1,
                            APELLIDO_2,
                            NOMBRE_1 + ' ' + NOMBRE_2 AS nombre,
                            CONVERT(varchar(10), FECHA_NACIMIENTO, 23) AS fecha_nac,
                            EstadoCivil_Desc,
                            Sexo_Desc,
                            Nacionalidad_Desc,
                            Institucion_Desc,
                            TEL_MOVIL,
                            TEL_HABITACION,
                            TEL_TRABAJO,
                            EMAIL_01,
                            EMAIL_02,
                            Provincia_Desc,
                            Canton_Desc,
                            Distrito_Desc,
                            DIRECCION,
                            REGISTRO_FECHA,
                            RESUELTO_FECHA,
                            RESUELTO_USUARIO,
                            I_POLIZA_VIDA_FAMILIAR,
                            I_AUTORIZACION_DEDUC
                        FROM vAFI_Afiliacion_EnLinea
                        WHERE 1 = 1
                        ";

                // ===== FILTROS (sbFiltro_Aplica) =====

                if (filtros.estado != "T")
                    sql += " AND Estado = @estado";

                if (filtros.tipo_fecha != "Todas")
                {
                    var campoFecha = filtros.tipo_fecha == "Registro"
                        ? "REGISTRO_FECHA"
                        : "RESUELTO_FECHA";

                    sql += $" AND {campoFecha} BETWEEN @inicio AND @corte";
                }

                if (!string.IsNullOrWhiteSpace(filtros.cedula))
                    sql += " AND CEDULA = @cedula";

                if (!string.IsNullOrWhiteSpace(filtros.id_alterno))
                    sql += " AND ID_COLILLA = @id_alterno";

                if (!string.IsNullOrWhiteSpace(filtros.nombre))
                    sql += @"
                         AND (APELLIDO_1 + ' ' + APELLIDO_2 + ' ' + NOMBRE_1 + ' ' + NOMBRE_2)
                             LIKE '%' + @nombre + '%'
                        ";

                sql += " ORDER BY SOLICITUD_ID";

                response.Result = cn.Query<AfiliacionTablaDto>(
                    sql,
                    new
                    {
                        estado = filtros.estado,
                        inicio = filtros.fecha_inicio?.Date,
                        corte = filtros.fecha_corte?.Date.AddDays(1).AddSeconds(-1),
                        filtros.cedula,
                        filtros.id_alterno,
                        filtros.nombre
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


        public ErrorDto<AfiliacionCasoDto?> Caso(int codEmpresa, long solicitudId)
        {
            var response = new ErrorDto<AfiliacionCasoDto?>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                    SELECT
                        SOLICITUD_ID                AS solicitud_id,
                        Estado                      AS estado,
                        Estado_Desc                 AS estado_desc,
                        Cedula                      AS cedula,
                        ID_COLILLA                  AS id_alterno,
                        APELLIDO_1                  AS apellido1,
                        APELLIDO_2                  AS apellido2,
                        NOMBRE_1                    AS nombre,
                        FECHA_NACIMIENTO            AS fecha_nac,
                        EstadoCivil_Desc            AS estado_civil,
                        Sexo_Desc                   AS genero,
                        Nacionalidad_Desc           AS nacionalidad,
                        Tel_Movil                   AS tel_movil,
                        TEL_HABITACION              AS tel_habitacion,
                        TEL_TRABAJO                 AS tel_trabajo,
                        EMAIL_01                    AS email_01,
                        EMAIL_02                    AS email_02,
                        Provincia_Desc              AS provincia,
                        Canton_Desc                 AS canton,
                        Distrito_Desc               AS distrito,
                        DIRECCION                   AS direccion,
                        Institucion_Desc            AS empresa,
                        FECHA_INGRESO_LABORAL       AS fecha_ingreso_empresa,
                        I_POLIZA_VIDA_FAMILIAR      AS poliza
                    FROM vAFI_Afiliacion_EnLinea
                    WHERE SOLICITUD_ID = @solicitudId
                    ";

                response.Result = cn.QueryFirstOrDefault<AfiliacionCasoDto>(
                    sql,
                    new { solicitudId }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<List<AfiliacionResumenDto>> Resumen(int codEmpresa,DateTime inicio,DateTime corte)
        {
            var response = new ErrorDto<List<AfiliacionResumenDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<AfiliacionResumenDto>(
                    "spAFI_Afiliacion_EnLinea_Resumen",
                    new { inicio, corte },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto Resolver(int codEmpresa,long solicitudId,string estado,string usuario)
        {
            var response = new ErrorDto();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    "spAFI_Afiliacion_EnLinea_Resolucion",
                    new
                    {
                        solicitudId,
                        estado,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
                );
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
