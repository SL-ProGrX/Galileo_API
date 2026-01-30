using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlSeguimientoBL
    {
        private readonly FrmCoControlSeguimientoDB Db;

        public FrmCoControlSeguimientoBL(IConfiguration config)
        {
            Db = new FrmCoControlSeguimientoDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Expedientes_Obtener(int CodEmpresa, string? texto)
        {
            return Db.CO_Expedientes_Obtener(CodEmpresa, texto);
        }


        public ErrorDto<List<DropDownListaGenericaModel>> CO_Gestiones_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Gestiones_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_CausasMora_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_CausasMora_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Arreglos_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CO_Arreglos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CoControlSegGestionInfoDto> CO_Gestion_Info_Obtener(int CodEmpresa, string cod_gestion, string usuario)
        {
            return Db.CO_Gestion_Info_Obtener(CodEmpresa, cod_gestion, usuario);
        }

        public ErrorDto<CoControlSegVenceRangoDto> CO_ControlSeguimiento_Vence_Rango_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_ControlSeguimiento_Vence_Rango_Obtener(CodEmpresa, usuario);

        }
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistGestiones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_HistGestiones_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistGestiones_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_HistGestiones_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistOficiales_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_HistOficiales_Lista_Obtener(CodEmpresa, parametros);

        }

        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistOficiales_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_HistOficiales_Lista_Export(CodEmpresa, parametros);

        }

        public ErrorDto CO_ControlSeguimiento_HistOficiales_Actualizar(int CodEmpresa, CoControlSegHistOficialActualizarDto data)
        {
            return Db.CO_ControlSeguimiento_HistOficiales_Actualizar(CodEmpresa, data);
        }
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Fiadores_Lista_Obtener(int CodEmpresa, string parametros, bool soloOperacionesAtrasadas)
        {
            return Db.CO_ControlSeguimiento_Fiadores_Lista_Obtener(CodEmpresa, parametros, soloOperacionesAtrasadas);

        }

        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Fiadores_Lista_Export(int CodEmpresa, string parametros, bool soloOperacionesAtrasadas)
        {
            return Db.CO_ControlSeguimiento_Fiadores_Lista_Export(CodEmpresa, parametros, soloOperacionesAtrasadas);
        }
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Comisiones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_Comisiones_Lista_Obtener(CodEmpresa, parametros);
        }
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Comisiones_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_Comisiones_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CO_ControlSeguimiento_Registrar(int CodEmpresa, CoControlSegRegistrarDto data)
        {
            return Db.CO_ControlSeguimiento_Registrar(CodEmpresa, data);
        }
        public ErrorDto<CoControlSegEstadoDto> CO_ControlSeguimiento_Estado_Obtener(int CodEmpresa, string cedula)
        {
            return Db.CO_ControlSeguimiento_Estado_Obtener(CodEmpresa, cedula);
        }
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistDetalle_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_HistDetalle_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistDetalle_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.CO_ControlSeguimiento_HistDetalle_Lista_Export(CodEmpresa, parametros);
        }


    }
}
