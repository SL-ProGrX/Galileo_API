using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndSeguridadTasaPreferencialBL
    {
        private readonly FrmFndSeguridadTasaPreferencialDB _db;
        public FrmFndSeguridadTasaPreferencialBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _db = new FrmFndSeguridadTasaPreferencialDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_SeguridadTasaPreferencial_Obtener(int CodEmpresa, string jFiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jFiltros) ?? new FiltrosLazyLoadData();
            return _db.Fnd_SeguridadTasaPreferencial_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<FndSeguridadTasaPreferenciaPlanData>> Fnd_SeguridadTasaPreferencial_Planes_Obtener(int CodEmpresa, string rol, string? filtro)
        {
            return _db.Fnd_SeguridadTasaPreferencial_Planes_Obtener(CodEmpresa, rol, filtro);
        }

        public ErrorDto<List<FndSeguridadTasaPreferenciaUsuarioData>> Fnd_SeguridadTasaPreferencial_Usuarios_Obtener(int CodEmpresa, string rol, string? filtro)
        {
            return _db.Fnd_SeguridadTasaPreferencial_Usuarios_Obtener(CodEmpresa, rol, filtro);
        }

        public ErrorDto Fnd_SeguridadTasaPreferencial_Guardar(int CodEmpresa, FndSeguridadTasaPreferencialDto row, string usuario)
        {
            return _db.Fnd_SeguridadTasaPreferencial_Guardar(CodEmpresa, row, usuario);
        }

        public ErrorDto Fnd_SeguridadTasaPreferencial_Eliminar(int CodEmpresa, string tp_rol, string usuario)
        {
            return _db.Fnd_SeguridadTasaPreferencial_Eliminar(CodEmpresa, tp_rol, usuario);
        }

        public ErrorDto Fnd_SeguridadTasaPreferencial_RolPlan_Actualizar(int CodEmpresa, FndSeguridadTasaPreferencialRolPlanDto data)
        {
            return _db.Fnd_SeguridadTasaPreferencial_RolPlan_Actualizar(CodEmpresa, data);
        }

        public ErrorDto Fnd_SeguridadTasaPreferencial_RolAutorizador_Actualizar(int CodEmpresa, FndSeguridadTasaPreferencialRolAutorizadorDto data)
        {
            return _db.Fnd_SeguridadTasaPreferencial_RolAutorizador_Actualizar(CodEmpresa, data);
        }
    }
}