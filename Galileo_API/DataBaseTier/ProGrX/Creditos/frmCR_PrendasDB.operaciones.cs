using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    /// <summary>
    /// Operaciones de consulta y actualización complementarias de garantías prendarias.
    /// </summary>
    public partial class FrmCrPrendasDb
    {
        /// <summary>
        /// Obtiene las prendas asociadas a una operación o expediente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consulta.</param>
        /// <param name="operacion">Número de operación del crédito.</param>
        /// <param name="expediente">Número de expediente del crédito.</param>
        /// <returns>Listado de prendas encontradas.</returns>
        public ErrorDto<List<CrPrendaListaData>> CrPrendas_Obtener(
            int codEmpresa,
            long operacion,
            string expediente)
        {
            const string sql = "exec spCrd_Prendas_List @Operacion, @Expediente;";

            return EjecutarListaMapeada(
                codEmpresa,
                sql,
                new { Operacion = operacion, Expediente = expediente ?? string.Empty },
                MapearPrendaLista);
        }

        /// <summary>
        /// Obtiene el detalle de una garantía prendaria.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consulta.</param>
        /// <param name="prendaId">Identificador de la prenda.</param>
        /// <returns>Detalle de la prenda seleccionada.</returns>
        public ErrorDto<CrPrendaDetalleData> CrPrendas_ObtenerDetalle(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Garantia_Load @PrendaId;";

            return EjecutarUnicoMapeado(codEmpresa, sql, new { PrendaId = prendaId }, MapearPrendaDetalle);
        }

        /// <summary>
        /// Registra o actualiza el avalúo de una garantía prendaria.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa donde se guarda.</param>
        /// <param name="request">Datos del avalúo y usuario responsable.</param>
        /// <returns>Resultado del registro del avalúo.</returns>
        public ErrorDto<string> CrPrendas_AvaluoGuardar(int codEmpresa, CrPrendaAvaluoGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Operacion_Prenda_Avaluo
                    @PrendaId,
                    @Inspector,
                    @ValorTotal,
                    @PorcCobertura,
                    @Cobertura,
                    @Notas,
                    @FechaInspeccion,
                    @ValorFiscal,
                    @ValorFinal,
                    @MontoExtras,
                    @PolizaFactor,
                    @PolizaFormaliza,
                    @PolizaRstPlan,
                    @Usuario;";

            return EjecutarRespuestaSp(
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Inspector = request.inspector ?? string.Empty,
                    ValorTotal = request.valor_total,
                    Cobertura = request.cobertura,
                    PorcCobertura = request.porc_cobertura,
                    Notas = request.notas ?? string.Empty,
                    FechaInspeccion = ConvertirFechaSql(request.fecha_inspeccion),
                    ValorFiscal = request.valor_fiscal,
                    ValorFinal = request.valor_total,
                    MontoExtras = request.monto_extras,
                    PolizaFactor = request.poliza_factor,
                    PolizaFormaliza = request.poliza_formaliza,
                    PolizaRstPlan = request.poliza_rst_plan,
                    Usuario = UsuarioNormalizado(request.usuario)
                },
                "Avalúo registrado satisfactoriamente!");
        }

        /// <summary>
        /// Registra o actualiza la información notarial de una garantía prendaria.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa donde se guarda.</param>
        /// <param name="request">Datos notariales y usuario responsable.</param>
        /// <returns>Resultado de la actualización notarial.</returns>
        public ErrorDto<string> CrPrendas_NotariadoGuardar(int codEmpresa, CrPrendaNotariadoGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Operacion_Prenda_Notariado
                    @PrendaId,
                    @Notario,
                    @Tomo,
                    @Folio,
                    @Usuario;";

            return EjecutarRespuestaSp(
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Notario = request.notario ?? string.Empty,
                    Tomo = request.tomo ?? string.Empty,
                    Folio = request.folio ?? string.Empty,
                    Usuario = UsuarioNormalizado(request.usuario)
                },
                "Información de notariado actualizada satisfactoriamente!");
        }

        /// <summary>
        /// Registra una nota de trámite para una garantía prendaria.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa donde se guarda.</param>
        /// <param name="request">Datos de la nota y usuario responsable.</param>
        /// <returns>Resultado del registro de la nota.</returns>
        public ErrorDto<string> CrPrendas_NotaGuardar(int codEmpresa, CrPrendaNotaGuardarRequest request)
        {
            const string sql = "exec spCrd_Prendas_Anotaciones_Add @PrendaId, @Nota, @Usuario;";

            return EjecutarRespuestaSp(
                codEmpresa,
                sql,
                new
                {
                    PrendaId = request.prenda_id,
                    Nota = request.nota ?? string.Empty,
                    Usuario = UsuarioNormalizado(request.usuario)
                },
                "Registro de notas de prenda realizado satisfactoriamente!");
        }

        /// <summary>
        /// Actualiza la asignación de una cobertura de póliza a la prenda.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa donde se guarda.</param>
        /// <param name="request">Cobertura, estado de asignación y usuario responsable.</param>
        /// <returns>Resultado de la actualización de cobertura.</returns>
        public ErrorDto<string> CrPrendas_PolizaCoberturaGuardar(
            int codEmpresa,
            CrPrendaPolizaCoberturaGuardarRequest request)
        {
            const string sql = @"
                exec spCrd_Prendas_Polizas_Add
                    @PrendaId,
                    @CoberturaId,
                    @Usuario,
                    @Asignado;";

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new
            {
                PrendaId = request.prenda_id,
                CoberturaId = request.id_prenda_cobertura,
                Usuario = UsuarioNormalizado(request.usuario),
                Asignado = request.asignado
            });

            return new ErrorDto<string>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result = resp.Code == -1 ? string.Empty : "Cobertura de póliza actualizada correctamente."
            };
        }

        /// <summary>
        /// Obtiene el histórico de pólizas externas de una prenda.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consulta.</param>
        /// <param name="prendaId">Identificador de la prenda.</param>
        /// <returns>Pólizas externas registradas con su auditoría.</returns>
        public ErrorDto<List<CrPrendaHistoricoPolizaData>> CrPrendas_PolizasExternasLista(
            int codEmpresa,
            long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Polizas_Externas_Lista @PrendaId;";

            return EjecutarListaMapeada(codEmpresa, sql, new { PrendaId = prendaId }, MapearHistoricoPoliza);
        }

        /// <summary>
        /// Obtiene el detalle de una póliza externa seleccionada por el usuario.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consulta.</param>
        /// <param name="prendaId">Identificador de la prenda.</param>
        /// <param name="polizaExtId">Identificador de la póliza externa.</param>
        /// <returns>Detalle de la póliza externa.</returns>
        public ErrorDto<CrPrendaDetalleData> CrPrendas_PolizaExternaLoad(
            int codEmpresa,
            long prendaId,
            int polizaExtId)
        {
            const string sql = "exec spCrd_Prendas_Polizas_Externas_Load @PrendaId, @PolizaExtId;";

            return EjecutarUnicoMapeado(
                codEmpresa,
                sql,
                new { PrendaId = prendaId, PolizaExtId = polizaExtId },
                MapearPolizaExternaDetalle);
        }
    }
}
