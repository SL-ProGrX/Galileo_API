using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Info

        /// <summary>
        /// Obtiene la información general de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CRConsultaInfoDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cédula.",
                    -1,
                    new CRConsultaInfoDto());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
                var parametros = new
                {
                    Cedula = cedula.Trim(),
                    Usuario = (usuario ?? string.Empty).Trim()
                };

                CRConsultaInfoDto info;
                using (var multi = connection.QueryMultiple(
                    "spAF_Persona_Consultas",
                    param: parametros,
                    commandType: CommandType.StoredProcedure))
                {
                    info = new CRConsultaInfoDto
                    {
                        Telefonos = multi.Read<AfTelefonoDto>().ToList(),
                        CuentasBancarias = multi.Read<AfCuentaBancariaDto>().ToList(),
                        Beneficiarios = multi.Read<AfPersonaBeneficiarioDto>().ToList(),
                        Tarjetas = multi.Read<AfTarjetaDto>().ToList(),
                        Localizaciones = multi.Read<AfDireccionDto>().ToList(),
                        Ingresos = multi.Read<AfPersonaIngresoDto>().ToList(),
                        Renuncias = multi.Read<AfPersonaRenunciaDto>().ToList(),
                        Liquidaciones = multi.Read<CRliquidacionDto>().ToList(),
                        Nombramientos = multi.Read<AfPersonaNombramientoDto>().ToList(),
                        Salarios = multi.Read<AfPersonaSalarioDto>().ToList(),
                        Emails = multi.Read<AfPersonaEmailDto>().ToList(),
                        Motivos = multi.Read<AfMotivosDto>().ToList(),
                        Canales = multi.Read<AfCanalesDto>().ToList(),
                        Preferencias = multi.Read<CrPreferenciaDto>().ToList(),
                        Bienes = multi.Read<AfBienDto>().ToList(),
                        Escolaridad = multi.Read<AfEscolaridadDto>().ToList(),
                        Relaciones = multi.Read<AfPersonaRelacionDto>().ToList()
                    };
                }

                const string liquidacionesSql = @"
                    SELECT
                        L.CONSEC AS consec,
                        L.FECLIQ AS fecliq,
                        L.ESTADOACTLIQ AS estadoactliq,
                        CASE
                            WHEN L.ESTADOACTLIQ = 'A' THEN 'Ren.Asociación'
                            ELSE 'Ren.Patronal'
                        END AS estadoactliqdesc,
                        L.ESTADOACTUAL AS estadoactual,
                        RTRIM(ISNULL(E.DESCRIPCION, '')) AS estadoactualdesc,
                        RTRIM(ISNULL(E.DESCRIPCION, '')) AS estadopersona,
                        CONCAT(
                            RTRIM(ISNULL(L.TDOCUMENTO, '')),
                            RTRIM(ISNULL(CONVERT(varchar(50), L.NDOCUMENTO), ''))
                        ) AS tdocumento,
                        L.UBICACION AS ubicacion,
                        CASE
                            WHEN L.UBICACION = 'C' THEN 'Contabilidad'
                            ELSE 'Tesorería'
                        END AS ubicaciondesc,
                        ISNULL(L.TNETO, 0) AS tneto,
                        L.ESTADO AS estado,
                        CASE
                            WHEN L.ESTADO = 'P' THEN 'Procesada'
                            ELSE 'Reversada'
                        END AS estadodesc
                    FROM LIQUIDACION L
                    INNER JOIN AFI_ESTADOS_PERSONA E
                        ON L.ESTADOACTUAL = E.COD_ESTADO
                    WHERE L.CEDULA = @Cedula
                    ORDER BY L.FECLIQ;";

                info.Liquidaciones = connection.Query<CRliquidacionDto>(
                    liquidacionesSql,
                    parametros).ToList();

                const string contactoSql = @"
                    SELECT
                        S.direccion,
                        RTRIM(ISNULL(Prov.Descripcion, '')) AS provincia,
                        RTRIM(ISNULL(Cant.Descripcion, '')) AS canton,
                        RTRIM(ISNULL(Dist.Descripcion, '')) AS distrito,
                        S.sexo,
                        S.fecha_nac,
                        S.estadoCivil,
                        ISNULL(Ec.Descripcion, '') AS estadocivil_desc,
                        S.AF_EMAIL AS email_01,
                        S.EMAIL_02 AS email_02,
                        S.FACEBOOK,
                        S.TWITTER,
                        S.LINKEDIN,
                        Ep.DESCRIPCION AS estadopersona,
                        S.FECHAINGRESO,
                        ISNULL(Nc.DESCRIPCION, '') AS nacionalidad,
                        DATEDIFF(YEAR, ISNULL(S.FECHA_NAC, dbo.MyGetdate()), dbo.MyGetdate()) AS edad
                    FROM socios S
                    LEFT JOIN Provincias Prov ON S.Provincia = Prov.Provincia
                    LEFT JOIN Cantones Cant ON S.Provincia = Cant.Provincia AND S.Canton = Cant.Canton
                    LEFT JOIN Distritos Dist ON S.Provincia = Dist.Provincia AND S.Canton = Dist.Canton AND S.distrito = Dist.distrito
                    LEFT JOIN SYS_ESTADO_CIVIL Ec ON S.EstadoCivil = Ec.Estado_Civil
                    LEFT JOIN AFI_ESTADOS_PERSONA Ep ON S.ESTADOACTUAL = Ep.COD_ESTADO
                    LEFT JOIN SYS_NACIONALIDADES Nc ON S.COD_NACIONALIDAD = Nc.COD_NACIONALIDAD
                    WHERE S.cedula = @Cedula;";

                const string estadoLaboralSql = @"
                    IF COL_LENGTH('dbo.Socios', 'UP') IS NOT NULL
                       AND OBJECT_ID('dbo.uprogramatica') IS NOT NULL
                       AND OBJECT_ID('dbo.utrabajo') IS NOT NULL
                    BEGIN
                        SELECT
                            I.descripcion AS institucion,
                            D.descripcion AS departamento,
                            X.UT_descripcion AS seccion,
                            S.NOMBRAMIENTO_FECHA AS fecha,
                            DATEDIFF(YEAR, S.NOMBRAMIENTO_FECHA, dbo.MyGetdate()) AS anioslaborados,
                            ISNULL(El.DESCRIPCION, 'No Indica') AS estadolaboral
                        FROM socios S
                        LEFT JOIN instituciones I ON S.cod_institucion = I.cod_institucion
                        LEFT JOIN uprogramatica D ON S.UP = D.codigo
                        LEFT JOIN utrabajo X ON S.UT = X.UT_codigo
                        LEFT JOIN AFI_ESTADO_LABORAL El ON S.ESTADOLABORAL = El.ESTADO_LABORAL
                        WHERE S.cedula = @Cedula;
                    END
                    ELSE
                    BEGIN
                        SELECT
                            I.descripcion AS institucion,
                            D.descripcion AS departamento,
                            X.descripcion AS seccion,
                            S.NOMBRAMIENTO_FECHA AS fecha,
                            DATEDIFF(YEAR, S.NOMBRAMIENTO_FECHA, dbo.MyGetdate()) AS anioslaborados,
                            ISNULL(El.DESCRIPCION, 'No Indica') AS estadolaboral
                        FROM socios S
                        LEFT JOIN instituciones I ON S.cod_institucion = I.cod_institucion
                        LEFT JOIN afDepartamentos D ON S.cod_institucion = D.cod_institucion AND S.cod_departamento = D.cod_departamento
                        LEFT JOIN afSecciones X ON S.cod_institucion = X.cod_institucion
                            AND S.cod_departamento = X.cod_departamento AND S.cod_seccion = X.cod_Seccion
                        LEFT JOIN AFI_ESTADO_LABORAL El ON S.ESTADOLABORAL = El.ESTADO_LABORAL
                        WHERE S.cedula = @Cedula;
                    END;";

                info.Contacto = connection.Query<AFPersonaDetalleDto>(contactoSql, parametros).ToList();
                info.EstadoLaboral = connection.Query<AFPersonaEstadoLaboralDto>(estadoLaboralSql, parametros).ToList();

                var polizas = connection.Query(
                    "SELECT COD_POLIZA, POLIZA_DESC FROM vPoliza_Catalogo").ToList();
                foreach (var poliza in polizas)
                {
                    var codigoPoliza = Convert.ToString(poliza.COD_POLIZA) ?? string.Empty;
                    var descripcionPoliza = Convert.ToString(poliza.POLIZA_DESC) ?? string.Empty;

                    info.BenePolizas.Add(new AFPersonaBenePolizaDto
                    {
                        linea = 0,
                        tipo_id = null,
                        poliza = codigoPoliza.Trim(),
                        poliza_desc = descripcionPoliza.Trim()
                    });

                    var beneficiariosPoliza = connection.Query<AFPersonaBenePolizaDto>(
                        "EXEC spPoliza_Persona_Beneficiarios @Cedula, @Poliza",
                        new { Cedula = cedula.Trim(), Poliza = codigoPoliza }).ToList();

                    foreach (var beneficiario in beneficiariosPoliza)
                    {
                        beneficiario.poliza = codigoPoliza;
                        beneficiario.poliza_desc = descripcionPoliza;
                    }

                    info.BenePolizas.AddRange(beneficiariosPoliza);
                }

                return info;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CRConsultaInfoDto())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar información de la persona.", result.Code.GetValueOrDefault(-1), new CRConsultaInfoDto());
        }

        /// <summary>
        /// Registra o elimina un canal de contacto de la persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="req">Datos serializados del canal y el movimiento.</param>
        /// <returns>Resultado del registro del canal.</returns>
        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string req)
        {
            AfCanalesDto request = JsonConvert.DeserializeObject<AfCanalesDto>(req) ?? new AfCanalesDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", request.cedula);
                p.Add("@Canal", request.canal_tipo.ToString("D2"));
                p.Add("@TipoMov", request.asignado ? "A" : "E");
                p.Add("@Usuario", request.registro_usuario);
                connection.Execute("dbo.spAFI_Persona_Canales_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar canales de la persona.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Registra bienes de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string req)
        {
            AfPersonaBienesRegistraDto request = JsonConvert.DeserializeObject<AfPersonaBienesRegistraDto>(req) ?? new AfPersonaBienesRegistraDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", request.Cedula);
                p.Add("@Codigo", FormatearCodigoCompuesto(request.CodBien));
                p.Add("@TipoMov", request.Asignado ? "A" : "E");
                p.Add("@Usuario", request.Usuario);
                connection.Execute("dbo.spAFI_Persona_Bienes_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar bienes de la persona.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Registra escolaridad de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            AfPersonaEscolaridadRegistraDto req = JsonConvert.DeserializeObject<AfPersonaEscolaridadRegistraDto>(request) ?? new AfPersonaEscolaridadRegistraDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", FormatearCodigoCompuesto(req.CodEscolaridad));
                p.Add("@TipoMov", req.Asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);
                connection.Execute("dbo.spAFI_Persona_Escolaridad_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar escolaridad de la persona.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra la preferencia de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Preferencia_Registra(int CodEmpresa, string request)
        {
            CrPreferenciaDto req = JsonConvert.DeserializeObject<CrPreferenciaDto>(request) ?? new CrPreferenciaDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", FormatearCodigoCompuesto(req.CodPreferencia.ToString()));
                p.Add("@TipoMov", req.asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);
                connection.Execute("dbo.spAFI_Persona_Preferencias_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar preferencia de la persona.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}
