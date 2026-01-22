using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmAhExcedentesCeBL
    {
        private readonly FrmAhExcedentesCeDB _db;

        public FrmAhExcedentesCeBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesCeDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Excedentes_Periodos_Lista(int codEmpresa)
            => _db.Excedentes_Periodos_Lista(codEmpresa);

        public ErrorDto<ExcedentesPeriodoValidaResult?> Excedentes_Periodo_Aplicaciones_Valida(int codEmpresa, string periodoId)
            => _db.Excedentes_Periodo_Aplicaciones_Valida(codEmpresa, periodoId);

        public ErrorDto<List<ExcedentesCasosEspecialesResult>> Excedentes_CasosEspeciales_Lista(int codEmpresa, int lineas, int periodoId)
            => _db.Excedentes_CasosEspeciales_Lista(codEmpresa, lineas, periodoId);

        public ErrorDto<List<ExcedentesCasosEspecialNuevoResult>> Excedentes_CasosEspecial_Nuevo_Lista(int codEmpresa, int periodoId)
            => _db.Excedentes_CasosEspecial_Nuevo_Lista(codEmpresa, periodoId);

        public ErrorDto<ExcedentesCasosEspecialDetalleResult?> Excedentes_CasosEspecial_Detalle(int codEmpresa, int periodoId, string cedula)
            => _db.Excedentes_CasosEspecial_Detalle(codEmpresa, periodoId, cedula);

        public ErrorDto<List<ExcedentesCasosEspecialSalidasCambioResult>> Excedentes_CasosEspecial_SalidasCambio_Lista(int codEmpresa)
            => _db.Excedentes_CasosEspecial_SalidasCambio_Lista(codEmpresa);

        public ErrorDto<ExcedentesPeriodoEstadoResult?> Excedentes_Periodo_Estado(int codEmpresa, int periodoId)
            => _db.Excedentes_Periodo_Estado(codEmpresa, periodoId);

        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_CasoEspecial_Add(int codEmpresa, ExcedentesCasoEspecialAddParams param)
            => _db.Excedentes_CasoEspecial_Add(codEmpresa, param);

        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_CasoEspecial_Delete(int codEmpresa, CasoEspecialBaseParams param)
            => _db.Excedentes_CasoEspecial_Delete(codEmpresa, param);

        public ErrorDto Excedentes_Mass_CE_Sube(int codEmpresa, ExcedentesMassCESubeParams param)
            => _db.Excedentes_Mass_CE_Sube(codEmpresa, param);

        public ErrorDto<ExcedentesMassValidaResult?> Excedentes_Mass_CE_Valida(int codEmpresa, int periodoId)
            => _db.Excedentes_Mass_CE_Valida(codEmpresa, periodoId);

        public ErrorDto<List<ExcedentesMassCEConsultaResult>> Excedentes_Mass_CE_Consulta(int codEmpresa, int periodoId)
            => _db.Excedentes_Mass_CE_Consulta(codEmpresa, periodoId);

        public ErrorDto Excedentes_Mass_CE_Procesa(int codEmpresa, int periodoId)
            => _db.Excedentes_Mass_CE_Procesa(codEmpresa, periodoId);

        public ErrorDto Excedentes_Mass_CS_Sube(int codEmpresa, ExcedentesMassCSSubeParams param)
            => _db.Excedentes_Mass_CS_Sube(codEmpresa, param);

        public ErrorDto<ExcedentesMassValidaResult?> Excedentes_Mass_CS_Valida(int codEmpresa, int periodoId)
            => _db.Excedentes_Mass_CS_Valida(codEmpresa, periodoId);

        public ErrorDto<List<ExcedentesMassCSConsultaResult>> Excedentes_Mass_CS_Consulta(int codEmpresa, int periodoId)
            => _db.Excedentes_Mass_CS_Consulta(codEmpresa, periodoId);

        public ErrorDto Excedentes_Mass_CS_Procesa(int codEmpresa, int periodoId)
            => _db.Excedentes_Mass_CS_Procesa(codEmpresa, periodoId);

        public ErrorDto<List<ExcedentesCasosEspecialesAplicadosResult>> Excedentes_CasosEspeciales_Aplicados(int codEmpresa, ExcedentesCasosEspecialesAplicadosParams param)
            => _db.Excedentes_CasosEspeciales_Aplicados(codEmpresa, param);

        public ErrorDto<List<ExcedentesCambioSalidaListaResult>> Excedentes_CambioSalida_Lista(int codEmpresa, ExcedentesCambioSalidaListaParams param)
            => _db.Excedentes_CambioSalida_Lista(codEmpresa, param);

        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Add(int codEmpresa, ExcedentesCambioSalidaAddParams param)
            => _db.Excedentes_Cambio_Salida_Add(codEmpresa, param);

        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Delete(int codEmpresa, ExcedentesCambioSalidaDeleteParams param)
            => _db.Excedentes_Cambio_Salida_Delete(codEmpresa, param);

        public ErrorDto<OperacionCasoEspecialResult?> Excedentes_Cambio_Salida_Autoriza(int codEmpresa, CasoEspecialBaseParams param)
            => _db.Excedentes_Cambio_Salida_Autoriza(codEmpresa, param);
    }
}
