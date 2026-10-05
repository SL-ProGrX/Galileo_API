using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslExpedienteDB
    {
        /// <summary>
        /// Obtiene el encabezado completo de un expediente.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <param name="codExpediente">C&#243;digo del expediente.</param>
        /// <returns>Datos del expediente.</returns>
        public ErrorDto<FslExpedienteDatos>
            FSL_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            if (codExpediente <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion,
                    new FslExpedienteDatos());
            }

            const string sql = """
                SELECT
                    Ex.COD_EXPEDIENTE AS cod_expediente,
                    RTRIM(ISNULL(Ex.COD_PLAN, '')) AS cod_plan,
                    RTRIM(ISNULL(Ex.COD_CAUSA, '')) AS cod_causa,
                    RTRIM(ISNULL(Ex.COD_COMITE, '')) AS cod_comite,
                    RTRIM(ISNULL(Ex.CEDULA, '')) AS cedula,
                    RTRIM(ISNULL(Ex.REFERENCIA_DOCUMENTO, ''))
                        AS referencia_documento,
                    RTRIM(ISNULL(Ex.REFERENCIA_NUMERO, ''))
                        AS referencia_numero,
                    RTRIM(ISNULL(Ex.PRESENTA_CEDULA, ''))
                        AS presenta_cedula,
                    RTRIM(ISNULL(Ex.PRESENTA_NOMBRE, ''))
                        AS presenta_nombre,
                    RTRIM(ISNULL(Ex.PRESENTA_NOTAS, ''))
                        AS presenta_notas,
                    ISNULL(Ex.MEMBRESIA_MESES, 0) AS membresia_meses,
                    ISNULL(Ex.MEMBRESIA_PORCENTAJE, 0)
                        AS membresia_porcentaje,
                    Ex.FECHA_ESTABLECE_CAUSA AS fecha_establece_causa,
                    RTRIM(ISNULL(Ex.NOTAS, '')) AS notas,
                    RTRIM(ISNULL(Ex.ENFERMEDAD_NOTAS, ''))
                        AS enfermedad_notas,
                    RTRIM(ISNULL(Ex.ENFERMEDAD_USUARIO, ''))
                        AS enfermedad_usuario,
                    Ex.ENFERMEDAD_FECHA AS enfermedad_fecha,
                    Ex.REGISTRO_FECHA AS registro_fecha,
                    RTRIM(ISNULL(Ex.REGISTRO_USUARIO, ''))
                        AS registro_usuario,
                    RTRIM(ISNULL(Ex.MODIFICA_USUARIO, ''))
                        AS modifica_usuario,
                    Ex.MODIFICA_FECHA AS modifica_fecha,
                    RTRIM(ISNULL(Ex.ESTADO, '')) AS estado,
                    RTRIM(ISNULL(Ex.RESOLUCION_ESTADO, ''))
                        AS resolucion_estado,
                    RTRIM(ISNULL(Ex.RESOLUCION_NOTAS, ''))
                        AS resolucion_notas,
                    Ex.RESOLUCION_FECHA AS resolucion_fecha,
                    RTRIM(ISNULL(Ex.RESOLUCION_USUARIO, ''))
                        AS resolucion_usuario,
                    ISNULL(Ex.TOTAL_DISPONIBLE, 0) AS total_disponible,
                    ISNULL(Ex.TOTAL_APLICADO, 0) AS total_aplicado,
                    ISNULL(Ex.TOTAL_SOBRANTE, 0) AS total_sobrante,
                    RTRIM(ISNULL(Ex.TIPO_DESEMBOLSO, ''))
                        AS tipo_desembolso,
                    RTRIM(ISNULL(Ex.TESORERIA_SOLICITUD, ''))
                        AS tesoreria_solicitud,
                    Ex.TESORERIA_FECHA AS tesoreria_fecha,
                    RTRIM(ISNULL(Ex.TESORERIA_USUARIO, ''))
                        AS tesoreria_usuario,
                    RTRIM(ISNULL(Ex.TESORERIA_REMESA, ''))
                        AS tesoreria_remesa,
                    RTRIM(ISNULL(Ex.COD_ENFERMEDAD, ''))
                        AS cod_enfermedad,
                    RTRIM(ISNULL(Ex.APL_TIPO_DOC, '')) AS apl_tipo_doc,
                    RTRIM(ISNULL(Ex.APL_NUM_DOC, '')) AS apl_num_doc,
                    RTRIM(ISNULL(Ex.FND_PLAN, '')) AS fnd_plan,
                    RTRIM(ISNULL(Ex.FND_CONTRATO, '')) AS fnd_contrato,
                    RTRIM(ISNULL(Ex.FND_TIPO_DOC, '')) AS fnd_tipo_doc,
                    RTRIM(ISNULL(Ex.FND_NUM_DOC, '')) AS fnd_num_doc,
                    RTRIM(ISNULL(Soc.NOMBRE, '')) AS nombre,
                    RTRIM(Pl.COD_PLAN) + ' - ' +
                    RTRIM(ISNULL(Pl.DESCRIPCION, '')) AS [plan],
                    RTRIM(Pc.COD_CAUSA) + ' - ' +
                        RTRIM(ISNULL(Pc.DESCRIPCION, '')) AS causa,
                    RTRIM(Te.COD_ENFERMEDAD) + ' - ' +
                        RTRIM(ISNULL(Te.DESCRIPCION, '')) AS enfermedad,
                    RTRIM(Co.COD_COMITE) + ' - ' +
                        RTRIM(ISNULL(Co.DESCRIPCION, '')) AS comite
                FROM FSL_EXPEDIENTES Ex
                INNER JOIN SOCIOS Soc
                    ON Ex.CEDULA = Soc.CEDULA
                INNER JOIN FSL_PLANES Pl
                    ON Ex.COD_PLAN = Pl.COD_PLAN
                INNER JOIN FSL_PLANES_CAUSAS Pc
                    ON Ex.COD_PLAN = Pc.COD_PLAN
                   AND Ex.COD_CAUSA = Pc.COD_CAUSA
                INNER JOIN FSL_TIPOS_ENFERMEDADES Te
                    ON Ex.COD_ENFERMEDAD = Te.COD_ENFERMEDAD
                INNER JOIN FSL_COMITES Co
                    ON Ex.COD_COMITE = Co.COD_COMITE
                WHERE Ex.COD_EXPEDIENTE = @cod_expediente;
                """;

            var respuesta = DbHelper.ExecuteSingleQuery(
                _portalDb,
                CodEmpresa,
                sql,
                new FslExpedienteDatos(),
                new { cod_expediente = codExpediente });

            if (respuesta.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    respuesta.Description ?? "Error al consultar el expediente.",
                    -1,
                    new FslExpedienteDatos());
            }

            if (respuesta.Result?.cod_expediente <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeExpedienteNoExiste,
                    CodigoValidacion,
                    new FslExpedienteDatos());
            }

            return DbHelper.CreateOkResponse(respuesta.Result);
        }

        /// <summary>
        /// Obtiene el expediente anterior o siguiente existente.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <param name="codExpediente">Expediente actual.</param>
        /// <param name="siguiente">Indica la direcci&#243;n.</param>
        /// <returns>C&#243;digo encontrado.</returns>
        public ErrorDto<long>
            FSL_Expediente_Navegacion_Obtener(
                int CodEmpresa,
                long codExpediente,
                bool siguiente)
        {
            const string sqlSiguiente = """
                SELECT TOP (1) COD_EXPEDIENTE
                FROM FSL_EXPEDIENTES
                WHERE COD_EXPEDIENTE > @cod_expediente
                ORDER BY COD_EXPEDIENTE;
                """;

            const string sqlAnterior = """
                SELECT TOP (1) COD_EXPEDIENTE
                FROM FSL_EXPEDIENTES
                WHERE COD_EXPEDIENTE < @cod_expediente
                ORDER BY COD_EXPEDIENTE DESC;
                """;

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                CodEmpresa,
                siguiente ? sqlSiguiente : sqlAnterior,
                codExpediente,
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene los requisitos del expediente.
        /// </summary>
        public ErrorDto<List<FslExpedienteRequisitoData>>
            FSL_Expediente_Requisitos_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    Ex.COD_REQUISITO AS cod_requisito,
                    Rq.DESCRIPCION AS descripcion,
                    CONVERT(bit, Ex.ESTADO) AS estado,
                    CONVERT(bit, Ex.OPCIONAL) AS opcional
                FROM FSL_EXPEDIENTES_REQUISITOS Ex
                INNER JOIN FSL_REQUISITOS Rq
                    ON Ex.COD_REQUISITO = Rq.COD_REQUISITO
                WHERE Ex.COD_EXPEDIENTE = @cod_expediente
                ORDER BY Ex.COD_REQUISITO;
                """;

            return DbHelper.ExecuteListQuery<FslExpedienteRequisitoData>(
                _portalDb,
                CodEmpresa,
                sql,
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene las operaciones del expediente.
        /// </summary>
        public ErrorDto<List<FslExpedienteOperacionData>>
            FSL_Expediente_Operaciones_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    E.ID_SOLICITUD AS id_solicitud,
                    RTRIM(ISNULL(E.REFERENCIA, '')) AS referencia,
                    RTRIM(ISNULL(Gar.DESCRIPCION, '')) AS descripcion,
                    ISNULL(R.PRIDEDUC, 0) AS prideduc,
                    ISNULL(R.MONTOAPR, 0) AS montoapr,
                    ISNULL(E.SALDO_CORTE, 0) AS saldo_corte,
                    ISNULL(E.MONTO_BASE, 0) AS monto_base,
                    ISNULL(E.PORC_RELACION, 0) AS porc_relacion,
                    RTRIM(ISNULL(E.TIPO_TABLA, '')) AS tipo_tabla,
                    ISNULL(E.PORCENTAJE, 0) AS porcentaje,
                    ISNULL(E.MONTO_RECONOCIMIENTO, 0)
                        AS monto_reconocimiento,
                    ISNULL(E.TIEMPO_TRANS, 0) AS tiempo_trans,
                    CASE
                        WHEN E.TIPO_BASE = 'S' THEN 'Saldo'
                        ELSE 'Mnt.Form.'
                    END AS base_calculo
                FROM FSL_EXPEDIENTES_DETALLE E
                INNER JOIN REG_CREDITOS R
                    ON E.ID_SOLICITUD = R.ID_SOLICITUD
                INNER JOIN CRD_GARANTIA_TIPOS Gar
                    ON R.GARANTIA = Gar.GARANTIA
                WHERE E.COD_EXPEDIENTE = @cod_expediente
                ORDER BY ISNULL(E.REFERENCIA, E.ID_SOLICITUD) DESC;
                """;

            return DbHelper.ExecuteListQuery<FslExpedienteOperacionData>(
                _portalDb,
                CodEmpresa,
                sql,
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene los miembros del comit&#233; del expediente.
        /// </summary>
        public ErrorDto<List<FslExpedienteResolucionMiembroData>>
            FSL_Expediente_ResolucionMiembros_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    RTRIM(Cm.CEDULA) AS cedula,
                    RTRIM(ISNULL(Cm.NOMBRE, '')) AS nombre,
                    ISNULL(Ec.ASIGNA_USUARIO, 'No!') AS asignado
                FROM FSL_EXPEDIENTES Ex
                INNER JOIN FSL_COMITES_MIEMBROS Cm
                    ON Ex.COD_COMITE = Cm.COD_COMITE
                LEFT JOIN FSL_EXPEDIENTE_COMITE Ec
                    ON Ex.COD_EXPEDIENTE = Ec.COD_EXPEDIENTE
                   AND Ex.COD_COMITE = Ec.COD_COMITE
                   AND Cm.CEDULA = Ec.CEDULA
                WHERE Ex.COD_EXPEDIENTE = @cod_expediente
                  AND Cm.ACTIVO = 1
                ORDER BY Cm.NOMBRE;
                """;

            return DbHelper.ExecuteListQuery<FslExpedienteResolucionMiembroData>(
                _portalDb,
                CodEmpresa,
                sql,
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene las validaciones necesarias para resolver.
        /// </summary>
        public ErrorDto<FslExpedienteResolucionValidacionesData>
            FSL_Expediente_ResolucionValidaciones_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    CONVERT(bit,
                        dbo.fxFSL_ExpedienteValidaRequisitos(
                            Ex.COD_EXPEDIENTE))
                        AS cumple_requisitos,
                    CONVERT(bit,
                        dbo.fxFSL_ExpedienteValidaTiempoPresentacion(
                            Ex.COD_EXPEDIENTE))
                        AS cumple_tiempo,
                    CONVERT(bit,
                        dbo.fxFSL_ExpedienteValidaRegistro(
                            Ex.CEDULA,
                            Ex.COD_PLAN,
                            Ex.COD_CAUSA,
                            Ex.COD_EXPEDIENTE))
                        AS cumple_registro
                FROM FSL_EXPEDIENTES Ex
                WHERE Ex.COD_EXPEDIENTE = @cod_expediente;
                """;

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                CodEmpresa,
                sql,
                new FslExpedienteResolucionValidacionesData(),
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene las gestiones del expediente.
        /// </summary>
        public ErrorDto<List<FslExpedienteGestionData>>
            FSL_Expediente_Gestiones_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(Tg.DESCRIPCION, '')) AS descripcion,
                    Eg.LINEA AS linea,
                    Eg.COD_EXPEDIENTE AS cod_expediente,
                    RTRIM(ISNULL(Eg.COD_GESTION, '')) AS cod_gestion,
                    RTRIM(ISNULL(Eg.NOTAS, '')) AS notas,
                    Eg.REGISTRO_FECHA AS registro_fecha,
                    RTRIM(ISNULL(Eg.REGISTRO_USUARIO, ''))
                        AS registro_usuario
                FROM FSL_EXPEDIENTE_GESTIONES Eg
                INNER JOIN FSL_TIPOS_GESTIONES Tg
                    ON Eg.COD_GESTION = Tg.COD_GESTION
                WHERE Eg.COD_EXPEDIENTE = @cod_expediente
                ORDER BY Eg.REGISTRO_FECHA DESC;
                """;

            return DbHelper.ExecuteListQuery<FslExpedienteGestionData>(
                _portalDb,
                CodEmpresa,
                sql,
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene las apelaciones del expediente.
        /// </summary>
        public ErrorDto<List<FslExpedienteApelacionData>>
            FSL_Expediente_Apelaciones_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(Ta.DESCRIPCION, '')) AS descripcion,
                    Ea.LINEA AS linea,
                    Ea.COD_EXPEDIENTE AS cod_expediente,
                    RTRIM(ISNULL(Ea.COD_APELACION, '')) AS cod_apelacion,
                    Ea.FECHA_APELACION AS fecha_apelacion,
                    RTRIM(ISNULL(Ea.PRESENTA_IDENTIFICACION, ''))
                        AS presenta_identificacion,
                    RTRIM(ISNULL(Ea.PRESENTA_NOMBRE, ''))
                        AS presenta_nombre,
                    RTRIM(ISNULL(Ea.NOTAS, '')) AS notas,
                    RTRIM(ISNULL(Ea.RESOLUCION, '')) AS resolucion,
                    RTRIM(ISNULL(Ea.REGISTRA_USUARIO, ''))
                        AS registra_usuario,
                    Ea.REGISTRA_FECHA AS registra_fecha,
                    Ea.RESOLUCION_FECHA AS resolucion_fecha,
                    RTRIM(ISNULL(Ea.RESOLUCION_USUARIO, ''))
                        AS resolucion_usuario,
                    RTRIM(ISNULL(Ea.RESOLUCION_NOTAS, ''))
                        AS resolucion_notas
                FROM FSL_EXPEDIENTES_APELACIONES Ea
                INNER JOIN FSL_TIPOS_APELACIONES Ta
                    ON Ea.COD_APELACION = Ta.COD_APELACION
                WHERE Ea.COD_EXPEDIENTE = @cod_expediente
                ORDER BY Ea.REGISTRA_FECHA DESC;
                """;

            return DbHelper.ExecuteListQuery<FslExpedienteApelacionData>(
                _portalDb,
                CodEmpresa,
                sql,
                new { cod_expediente = codExpediente });
        }

        /// <summary>
        /// Obtiene el usuario vinculado a un miembro.
        /// </summary>
        public ErrorDto<string>
            FSL_Expediente_UsuarioVinculado_Obtener(
                int CodEmpresa,
                string cedula,
                string codComite)
        {
            if (string.IsNullOrWhiteSpace(cedula) ||
                string.IsNullOrWhiteSpace(codComite))
            {
                return DbHelper.CreateErrorResponse(
                    "El miembro y el comit&eacute; son requeridos.",
                    CodigoValidacion,
                    string.Empty);
            }

            const string sql = """
                SELECT RTRIM(ISNULL(USUARIO_VINCULADO, ''))
                FROM FSL_COMITES_MIEMBROS
                WHERE CEDULA = @cedula
                  AND COD_COMITE = @cod_comite;
                """;

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                CodEmpresa,
                sql,
                string.Empty,
                new
                {
                    cedula = cedula.Trim(),
                    cod_comite = codComite.Trim()
                });
        }
    }
}