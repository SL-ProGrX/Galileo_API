using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslExpedienteDB
    {
        /// <summary>
        /// Obtiene el detalle de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Datos del expediente.</returns>
        public ErrorDto<FslExpedienteDatos> FslExpediente_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Ex.*, Soc.NOMBRE AS nombre,
                                            Pl.COD_PLAN + ' - ' + RTRIM(Pl.DESCRIPCION) AS 'plan',
                                            Pc.COD_CAUSA + ' - ' + RTRIM(Pc.DESCRIPCION) AS 'causa',
                                            Te.COD_ENFERMEDAD + ' - ' + RTRIM(Te.DESCRIPCION) AS 'enfermedad',
                                            Co.COD_COMITE + ' - ' + RTRIM(Co.DESCRIPCION) AS 'comite'
                                     FROM FSL_EXPEDIENTES Ex
                                     INNER JOIN SOCIOS Soc ON Ex.Cedula = Soc.Cedula
                                     INNER JOIN FSL_PLANES Pl ON Ex.COD_PLAN = Pl.COD_PLAN
                                     INNER JOIN FSL_PLANES_CAUSAS Pc ON Ex.COD_PLAN = Pc.COD_PLAN AND Ex.COD_CAUSA = Pc.COD_CAUSA
                                     INNER JOIN FSL_TIPOS_ENFERMEDADES Te ON Ex.COD_ENFERMEDAD = Te.COD_ENFERMEDAD
                                     INNER JOIN FSL_COMITES Co ON Ex.COD_COMITE = Co.COD_COMITE
                                     WHERE Ex.COD_EXPEDIENTE = @cod_expediente";
                return connection.QueryFirstOrDefault<FslExpedienteDatos>(sql, new { cod_expediente }) ?? new FslExpedienteDatos();
            });
        }

        /// <summary>
        /// Obtiene los requisitos de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Lista de requisitos.</returns>
        public ErrorDto<List<FslRequisitosExp>> FslRequisitos_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Ex.COD_REQUISITO AS cod_requisito, Rq.DESCRIPCION AS descripcion, EX.Estado AS estado, Ex.Opcional AS opcional
                                     FROM FSL_EXPEDIENTES_REQUISITOS Ex
                                     INNER JOIN FSL_REQUISITOS Rq ON Ex.cod_requisito = Rq.cod_requisito
                                     WHERE Ex.cod_Expediente = @cod_expediente";
                return connection.Query<FslRequisitosExp>(sql, new { cod_expediente }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las operaciones (créditos) de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Lista de operaciones.</returns>
        public ErrorDto<List<FslOperacionesDatos>> FslOperaciones_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT E.ID_SOLICITUD AS id_solicitud, E.REFERENCIA AS referencia, Gar.DESCRIPCION AS descripcion,
                                            R.PRIDEDUC AS prideduc, R.MONTOAPR AS montoapr, E.SALDO_CORTE AS saldo_corte, E.MONTO_BASE AS monto_base,
                                            E.PORC_RELACION AS porc_relacion, E.TIPO_TABLA AS tipo_tabla, E.PORCENTAJE AS porcentaje,
                                            E.MONTO_RECONOCIMIENTO AS monto_reconocimiento, E.TIEMPO_TRANS AS tiempo_trans,
                                            CASE WHEN E.Tipo_Base = 'S' THEN 'Saldo' ELSE 'Mnt.Form.' END AS _base
                                     FROM FSL_EXPEDIENTES_DETALLE E
                                     INNER JOIN REG_CREDITOS R ON E.ID_SOLICITUD = R.ID_SOLICITUD
                                     INNER JOIN CRD_GARANTIA_TIPOS Gar ON R.GARANTIA = Gar.GARANTIA
                                     WHERE E.COD_EXPEDIENTE = @cod_expediente
                                     ORDER BY ISNULL(E.referencia, E.id_Solicitud) DESC";
                return connection.Query<FslOperacionesDatos>(sql, new { cod_expediente }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la resolución (miembros del comité asignados) de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Lista de miembros y su asignación.</returns>
        public ErrorDto<List<FslResolucionDatos>> FslResolucion_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Cm.Cedula AS cedula, Cm.Nombre AS nombre, ISNULL(Ec.Asigna_Usuario, 'No!') AS asignado
                                     FROM FSL_EXPEDIENTES Ex
                                     INNER JOIN FSL_COMITES_MIEMBROS Cm ON Ex.COD_COMITE = Cm.COD_COMITE
                                     LEFT JOIN FSL_EXPEDIENTE_COMITE Ec ON Ex.COD_EXPEDIENTE = Ec.COD_EXPEDIENTE AND Ex.COD_COMITE = Ec.COD_COMITE
                                            AND Cm.Cedula = Ex.Cedula
                                     WHERE Ex.cod_Expediente = @cod_expediente AND Cm.Activo = 1";
                return connection.Query<FslResolucionDatos>(sql, new { cod_expediente }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las validaciones de la resolución de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Validaciones de la resolución.</returns>
        public ErrorDto<List<FslResolucionValidacionesDatos>> FslResolucionlVal_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT dbo.fxFSL_ExpedienteValidaRequisitos(Ex.Cod_Expediente) AS CumpleRequisitos,
                                            dbo.fxFSL_ExpedienteValidaTiempoPresentacion(Ex.Cod_Expediente) AS CumpleTiempo,
                                            dbo.fxFSL_ExpedienteValidaRegistro(Ex.Cedula, Ex.Cod_Plan, Ex.Cod_Causa, Ex.Cod_Expediente) AS CumpleRegistro
                                     FROM FSL_EXPEDIENTES Ex
                                     WHERE Ex.COD_EXPEDIENTE = @cod_expediente";
                return connection.Query<FslResolucionValidacionesDatos>(sql, new { cod_expediente }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las gestiones de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Lista de gestiones.</returns>
        public ErrorDto<List<FslExpGestiones>> FslExpGestiones_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Tg.Descripcion AS descripcion, Eg.*
                                     FROM FSL_EXPEDIENTE_GESTIONES Eg
                                     INNER JOIN FSL_TIPOS_GESTIONES Tg ON Eg.COD_GESTION = Tg.COD_GESTION
                                     WHERE Eg.cod_Expediente = @cod_expediente ORDER BY registro_fecha DESC";
                return connection.Query<FslExpGestiones>(sql, new { cod_expediente }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las apelaciones de un expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <returns>Lista de apelaciones.</returns>
        public ErrorDto<List<FslApelacionDatos>> FslApelaciones_Obtener(int CodCliente, int cod_expediente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Ta.Descripcion, Ea.*
                                     FROM FSL_EXPEDIENTES_APELACIONES Ea
                                     INNER JOIN FSL_TIPOS_APELACIONES Ta ON Ea.COD_APELACION = Ta.COD_APELACION
                                     WHERE Ea.cod_Expediente = @cod_expediente ORDER BY registra_fecha DESC";
                return connection.Query<FslApelacionDatos>(sql, new { cod_expediente }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de expedientes con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por código o cédula.</param>
        /// <returns>Lista de expedientes y total.</returns>
        public ErrorDto<FslExpedienteListaData> FslExpedientesLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslExpedienteListaData();

                const string sqlCount = "SELECT COUNT(COD_EXPEDIENTE) FROM FSL_EXPEDIENTES";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT COD_EXPEDIENTE AS cod_expediente, CEDULA AS cedula
                                     FROM FSL_EXPEDIENTES
                                     WHERE (@like IS NULL OR COD_EXPEDIENTE LIKE @like OR CEDULA LIKE @like)
                                     ORDER BY COD_EXPEDIENTE
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.expediente = connection.Query<FslExpedienteData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Valida si un caso ya fue presentado anteriormente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <param name="tipo">Tipo (plan).</param>
        /// <param name="causa">Causa.</param>
        /// <returns>Resultado de la validación.</returns>
        public ErrorDto FslExpediente_Valida(int CodCliente, string cedula, string tipo, string causa)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sql = "SELECT dbo.fxFSL_ExpedienteValidaRegistro(@cedula, @tipo, @causa, 0) AS Cumple";
                var cumple = connection.QueryFirstOrDefault<long>(sql, new { cedula, tipo, causa });

                return cumple == 0
                    ? DbHelper.ErrorResponse("- El caso ya fue presentado anteriormente...verifique!")
                    : new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el usuario vinculado de un miembro de comité.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cedula">Cédula del miembro.</param>
        /// <param name="cod_comite">Código del comité.</param>
        /// <returns>Usuario vinculado en Description.</returns>
        public ErrorDto FslUsuarioVinculado_Obtener(int CodCliente, string cedula, string cod_comite)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sql = @"SELECT usuario_Vinculado FROM FSL_Comites_Miembros
                                     WHERE cedula = @cedula AND cod_comite = @cod_comite";
                var usuario = connection.Query<string>(sql, new { cedula, cod_comite }).LastOrDefault();

                return new ErrorDto { Code = 0, Description = usuario ?? string.Empty };
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("No fue posible obtener el usuario vinculado");
            }
        }
    }
}
