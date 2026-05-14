using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndAlertasParametrosBL
    {
        private readonly FrmFndAlertasParametrosDB _db;
        public FrmFndAlertasParametrosBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _db = new FrmFndAlertasParametrosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_Operadora_Obtener(int CodEmpresa)
        {
            return _db.Fnd_AlertasParametros_Operadora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_Planes_Obtener(int CodEmpresa, string operadora)
        {
            return _db.Fnd_AlertasParametros_Planes_Obtener(CodEmpresa, operadora);
        }

        public ErrorDto<string> Fnd_AlertasParametros_Plan_Obtener(int CodEmpresa, string operadora, string plan)
        {
            return _db.Fnd_AlertasParametros_Plan_Obtener(CodEmpresa, operadora, plan);
        }

        #region ALERTAS
        public ErrorDto<FndalertasData> Fnd_AlertasPlanes_Scroll_Obtener(int codEmpresa, int codOperadora, string codPlanActual, bool siguiente)
        {
            return _db.Fnd_AlertasPlanes_Scroll_Obtener(codEmpresa, codOperadora, codPlanActual, siguiente);
        }

        public ErrorDto<FndalertasData> Fnd_AlertasParametros_Alerta_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _db.Fnd_AlertasParametros_Alerta_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_AlertasParametros_Lista_Obtener(int CodEmpresa, string jFiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jFiltros) ?? new FiltrosLazyLoadData();
            return  _db.Fnd_AlertasParametros_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_NuevoPlan_Obtener(int CodEmpresa, string operadora)
        {
            return _db.Fnd_AlertasParametros_NuevoPlan_Obtener(CodEmpresa, operadora);
        }

        public ErrorDto Fnd_AlertasParametros_Registrar(int codEmpresa, string usuario, FndalertasData alerta)
        {
            return _db.Fnd_AlertasParametros_Registrar(codEmpresa, usuario, alerta);
        }

        public ErrorDto Fnd_AlertasParametros_Alerta_Guardar(int CodEmpresa, FndalertasData data)
        {
            return _db.Fnd_AlertasParametros_Alerta_Guardar(CodEmpresa, data);
        }

        #endregion

        #region EMAIL

        public ErrorDto Fnd_AlertasEmail_Guardar(int codEmpresa, FndAlertasContactosDto contacto)
        {
            return _db.Fnd_AlertasEmail_Guardar(codEmpresa, contacto);
        }

        public ErrorDto Fnd_AlertasEmail_Eliminar(int codEmpresa, string usuario, List<FndAlertasContactosDto> listaContactos)
        {
            return _db.Fnd_AlertasEmail_Eliminar(codEmpresa, usuario, listaContactos);
        }

        public ErrorDto Fnd_AlertasEmailId_Eliminar(int codEmpresa, string usuario, int idregistro)
        {
            return _db.Fnd_AlertasEmailId_Eliminar(codEmpresa, usuario, idregistro);
        }

        #endregion

    }
}