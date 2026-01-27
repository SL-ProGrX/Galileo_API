using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmAhExcedentesCeDB
    {
        private readonly PortalDB _portalDb;

        public FrmAhExcedentesCeDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de periodos de excedentes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de periodos en formato genérico para dropdown.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Excedentes_Periodos_Lista(int codEmpresa)
        {
            var query = "SELECT IdX as item, ItmX as descripcion FROM vExc_Periodos ORDER BY IdX DESC";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Valida si el periodo tiene aplicaciones de excedentes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Resultado de la validación.</returns>
        public ErrorDto<ExcedentesPeriodoValidaResult?> Excedentes_Periodo_Aplicaciones_Valida(int codEmpresa, string periodoId)
        {
            var query = "SELECT dbo.fxExc_Periodo_Aplicaciones_Valida(@PeriodoId) AS Resultado";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteSingleQuery<ExcedentesPeriodoValidaResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Obtiene la lista de casos especiales por periodo.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="lineas">Cantidad máxima de registros.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Lista de casos especiales.</returns>
        public ErrorDto<List<ExcedentesCasosEspecialesResult>> Excedentes_CasosEspeciales_Lista(int codEmpresa, int lineas, int periodoId)
        {
            var query = @"SELECT TOP (@Lineas)
                A.CONSEC AS Consec,
                A.ID_PERIODO AS Id_Periodo,
                A.CEDULA AS Cedula,
                A.NOMBRE AS Nombre,
                A.SALIDA AS Salida,
                A.DETALLE AS Detalle,
                A.PORCENTAJE AS Porcentaje,
                A.DOC_ADJUNTO AS Doc_Adjunto,
                A.REGISTRO_USUARIO AS Registro_Usuario,
                A.REGISTRO_FECHA AS Registro_Fecha,
                A.MODIFICA_USUARIO AS Modifica_Usuario,
                A.MODIFICA_FECHA AS Modifica_Fecha,
                A.DOC_AJUNTO AS Doc_Ajunto,
                S.Nombre AS Socio_Nombre
            FROM EXC_CASOS_ESPECIALES A
            INNER JOIN Socios S ON A.CEDULA = S.CEDULA
            WHERE A.ID_PERIODO = @PeriodoId";
            var parameters = new { Lineas = lineas, PeriodoId = periodoId };
            return DbHelper.ExecuteListQuery<ExcedentesCasosEspecialesResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene la lista de casos especiales nuevos por periodo.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Lista de casos especiales nuevos.</returns>
        public ErrorDto<List<ExcedentesCasosEspecialNuevoResult>> Excedentes_CasosEspecial_Nuevo_Lista(int codEmpresa, int periodoId)
        {
            var query = @"SELECT Cedula, Cedular, Nombre
                          FROM vExc_Casos_Especial_Nuevo
                          WHERE ID_PERIODO = @PeriodoId";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteListQuery<ExcedentesCasosEspecialNuevoResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene el detalle de un caso especial por periodo y cédula.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns>Detalle del caso especial.</returns>
        public ErrorDto<ExcedentesCasosEspecialDetalleResult?> Excedentes_CasosEspecial_Detalle(int codEmpresa, int periodoId, string cedula)
        {
            var query = @"SELECT TOP 1
                            A.Cedula,
                            S.Nombre,
                            A.Salida,
                            A.Detalle,
                            CASE WHEN A.Doc_Ajunto IS NULL THEN 0 ELSE 1 END AS Adjunto
                          FROM EXC_CASOS_ESPECIALES A
                          INNER JOIN Socios S ON A.Cedula = S.Cedula
                          WHERE A.ID_PERIODO = @PeriodoId
                            AND A.Cedula = @Cedula
                          ORDER BY A.CONSEC DESC";
            var parameters = new { PeriodoId = periodoId, Cedula = cedula };
            return DbHelper.ExecuteSingleQuery<ExcedentesCasosEspecialDetalleResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Obtiene la lista de salidas de cambio que requieren porcentaje.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de salidas de cambio.</returns>
        public ErrorDto<List<ExcedentesCasosEspecialSalidasCambioResult>> Excedentes_CasosEspecial_SalidasCambio_Lista(int codEmpresa)
        {
            var query = @"SELECT COD_SALIDA AS Cod_Salida, DESCRIPCION
                          FROM vExc_Casos_Especial_Salidas_Cambio
                          WHERE REQUIERE_PORCENTAJE = 1";
            return DbHelper.ExecuteListQuery<ExcedentesCasosEspecialSalidasCambioResult>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el estado de un periodo de excedentes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Estado del periodo.</returns>
        public ErrorDto<ExcedentesPeriodoEstadoResult?> Excedentes_Periodo_Estado(int codEmpresa, int periodoId)
        {
            var query = "SELECT Estado FROM Exc_Periodos WHERE Id_Periodo = @PeriodoId";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteSingleQuery<ExcedentesPeriodoEstadoResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Inserta o actualiza un caso especial.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del caso especial.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_CasoEspecial_Add(int codEmpresa, ExcedentesCasoEspecialAddParams param)
        {
            var query = "exec spExc_Caso_Especial_Add @IdCE, @PeriodoId, @Cedula, @Detalle, @Porcentaje, @Salida, @Usuario";
            var parameters = new
            {
                IdCE = param.Id,
                param.PeriodoId,
                param.Cedula,
                param.Detalle,
                param.Porcentaje,
                param.Salida,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<OperacionCasoEspecialResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Elimina un caso especial.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del caso especial a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_CasoEspecial_Delete(int codEmpresa, CasoEspecialBaseParams param)
        {
            var query = "exec spExc_Caso_Especial_Delete @IdCE, @PeriodoId, @Cedula, @Usuario";
            var parameters = new
            {
                IdCE = param.Id,
                param.PeriodoId,
                param.Cedula,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<OperacionCasoEspecialResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Sube un caso especial masivo (CE) al periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la carga masiva CE.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Excedentes_Mass_CE_Sube(int codEmpresa, ExcedentesMassCESubeParams param)
        {
            var query = "exec spEXC_Mass_CE_Sube @PeriodoId, 'CE', @Cedula, @Nombre, @Salida, @Porcentaje, @Detalle, @Usuario, @Primero";
            var parameters = new
            {
                param.PeriodoId,
                param.Cedula,
                param.Nombre,
                param.Salida,
                param.Porcentaje,
                param.Detalle,
                param.Usuario,
                param.Primero
            };
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Valida los casos especiales masivos (CE) para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Totales de la validación.</returns>
        public ErrorDto<ExcedentesMassValidaResult?> Excedentes_Mass_CE_Valida(int codEmpresa, int periodoId)
        {
            var query = "exec spEXC_Mass_CE_Valida @PeriodoId, 'CE'";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteSingleQuery<ExcedentesMassValidaResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Consulta los casos especiales masivos (CE) no procesados para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Lista de casos especiales masivos CE.</returns>
        public ErrorDto<List<ExcedentesMassConsultaBaseResult>> Excedentes_Mass_CE_Consulta(int codEmpresa, int periodoId)
        {
            var query = "exec spEXC_Mass_CE_Consulta @PeriodoId, 'CE'";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteListQuery<ExcedentesMassConsultaBaseResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Aplica los casos especiales masivos (CE) para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Excedentes_Mass_CE_Procesa(int codEmpresa, int periodoId)
        {
            var query = "exec spEXC_Mass_CE_Procesa @PeriodoId, 'CE'";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Sube un cambio de salida masivo (CS) al periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la carga masiva CS.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Excedentes_Mass_CS_Sube(int codEmpresa, ExcedentesMassCSSubeParams param)
        {
            var query = "exec spEXC_Mass_CS_Sube @PeriodoId, 'CS', @Cedula, @Nombre, @Salida, @Detalle, @Autoriza_Ind, @Autoriza_Usuario, @Usuario, @Primero";
            var parameters = new
            {
                param.PeriodoId,
                param.Cedula,
                param.Nombre,
                param.Salida,
                param.Detalle,
                param.Autoriza_Ind,
                param.Autoriza_Usuario,
                param.Usuario,
                param.Primero
            };
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Valida los cambios de salida masivos (CS) para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Totales de la validación.</returns>
        public ErrorDto<ExcedentesMassValidaResult?> Excedentes_Mass_CS_Valida(int codEmpresa, int periodoId)
        {
            var query = "exec spEXC_Mass_CS_Valida @PeriodoId, 'CS'";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteSingleQuery<ExcedentesMassValidaResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Consulta los cambios de salida masivos (CS) no procesados para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Lista de cambios de salida masivos CS.</returns>
        public ErrorDto<List<ExcedentesMassCSConsultaResult>> Excedentes_Mass_CS_Consulta(int codEmpresa, int periodoId)
        {
            var query = "exec spEXC_Mass_CS_Consulta @PeriodoId, 'CS'";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteListQuery<ExcedentesMassCSConsultaResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Aplica los cambios de salida masivos (CS) para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">ID del periodo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Excedentes_Mass_CS_Procesa(int codEmpresa, int periodoId)
        {
            var query = "exec spEXC_Mass_CS_Procesa @PeriodoId, 'CS'";
            var parameters = new { PeriodoId = periodoId };
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Consulta la lista de casos especiales aplicados según filtros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>Lista de casos especiales aplicados.</returns>
        public ErrorDto<List<ExcedentesCasosEspecialesAplicadosResult>> Excedentes_CasosEspeciales_Aplicados(int codEmpresa, ExcedentesCasosEspecialesAplicadosParams param)
        {
            var query = @"exec spExc_Casos_Especiales_Aplicados @PeriodoId, @Salida, @Cedula, @Nombre, @Detalle, @Usuario";
            var parameters = new
            {
                param.PeriodoId,
                param.Salida,
                param.Cedula,
                param.Nombre,
                param.Detalle,
                param.Usuario
            };
            return DbHelper.ExecuteListQuery<ExcedentesCasosEspecialesAplicadosResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Lista los casos de cambio de salida según filtros.
        /// </summary>
        public ErrorDto<List<ExcedentesCambioSalidaListaResult>> Excedentes_CambioSalida_Lista(int codEmpresa, ExcedentesCambioSalidaListaParams param)
        {
            var query = "exec spExc_CambioSalida_Lista @PeriodoId, @Filtro, @Autorizado, @Usuario";
            var parameters = new
            {
                param.PeriodoId,
                param.Filtro,
                param.Autorizado,
                param.Usuario
            };
            return DbHelper.ExecuteListQuery<ExcedentesCambioSalidaListaResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Inserta o actualiza un cambio de salida.
        /// </summary>
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Add(int codEmpresa, ExcedentesCambioSalidaAddParams param)
        {
            var query = "exec spExc_Cambio_Salida_Add @IdCS, @PeriodoId, @Cedula, @Detalle, @Salida, @Usuario";
            var parameters = new
            {
                IdCS = param.Id,
                param.PeriodoId,
                param.Cedula,
                param.Detalle,
                param.Salida,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<OperacionCasoEspecialResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Elimina un cambio de salida.
        /// </summary>
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Delete(int codEmpresa, ExcedentesCambioSalidaDeleteParams param)
        {
            var query = "exec spExc_Cambio_Salida_Delete @IdCS, @PeriodoId, @Cedula, @Salida, @Usuario";
            var parameters = new
            {
                IdCS = param.Id,
                param.PeriodoId,
                param.Cedula,
                param.Salida,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<OperacionCasoEspecialResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Autoriza un cambio de salida.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de autorización.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Autoriza(int codEmpresa, CasoEspecialBaseParams param)
        {
            var query = "exec spExc_Cambio_Salida_Autoriza @IdCS, @PeriodoId, @Cedula, @Usuario";
            var parameters = new
            {
                IdCS = param.Id,
                param.PeriodoId,
                param.Cedula,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<OperacionCasoEspecialResult>(_portalDb, codEmpresa, query, default, parameters);
        }

    }
}
