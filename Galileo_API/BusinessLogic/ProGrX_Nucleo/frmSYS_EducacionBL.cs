using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysEducacionBL(IConfiguration config)
    {
        private readonly FrmSysEducacionDB _db = new FrmSysEducacionDB(config);

        public ErrorDto<SysEducacionLista> Sys_EducacionlLista_Obtener(int CodEmpresa, string tipo, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
                throw new ArgumentException("Los filtros no pueden ser nulos.", nameof(jfiltros));
            return _db.Sys_EducacionlLista_Obtener(CodEmpresa, tipo, filtros);
        }

        public ErrorDto Sys_Educacion_Guardar(int CodEmpresa, string usuario, SysEducacionData datos)
        {
            return _db.Sys_Educacion_Guardar(CodEmpresa, usuario, datos);
        }

        public ErrorDto Sys_Educacion_Eliminar(int CodEmpresa, string usuario, string cod_Educ, string tipo)
        {
            return _db.Sys_Educacion_Eliminar(CodEmpresa, usuario, cod_Educ, tipo);
        }

        public ErrorDto<List<SysEducacionDetalleData>> Sys_EducacionDetalle_Consulta(int CodEmpresa, string tipoDetalleEduc, string tipoEduc)
        {
            return _db.Sys_EducacionDetalle_Consulta(CodEmpresa, tipoDetalleEduc, tipoEduc);
        }

        public ErrorDto Sys_EducacionDetalle_Asignar(int CodEmpresa, string usuario, string cod_Educ, string cod_DetalleEduc, bool accion)
        {
            return _db.Sys_EducacionDetalle_Asignar(CodEmpresa, usuario, cod_Educ, cod_DetalleEduc, accion);
        } 

    }
}