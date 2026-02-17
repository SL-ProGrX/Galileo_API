using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo; 

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifOficinasBL(IConfiguration config)
    {
        private readonly FrmSifOficinasBD _db = new FrmSifOficinasBD(config);

        public ErrorDto<SifOficinasLista> Sif_OficinasLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sif_OficinasLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Sif_Oficinas_Guardar(int CodEmpresa, SifOficinasData oficinaDatos)        {
           
            return _db.Sif_Oficinas_Guardar(CodEmpresa, oficinaDatos);
        }

        public ErrorDto Sif_Oficinas_ActualizarDatos(int CodEmpresa, SifOficinasData oficinaDatos)
        { 
            return _db.Sif_Oficinas_ActualizarDatos(CodEmpresa, oficinaDatos);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasUnidadContable_Obtener(int CodEmpresa)
        {
            return _db.Sif_OficinasUnidadContable_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasCentroCostos_Obtener(int CodEmpresa)
        {
            return _db.Sif_OficinasCentroCostos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sif_Oficinas_Lista(int CodEmpresa)
        {
            return _db.Sif_Oficinas_Lista(CodEmpresa);
        }

        public ErrorDto<List<SifOficinasMiembros>> Sif_OficinasMiembros_Lista(int CodEmpresa, string oficina, string filtro, int apoyo, int usuariosEstado)
        {
            return _db.Sif_OficinasMiembros_Lista(CodEmpresa,oficina, filtro, apoyo, usuariosEstado);
        }
      
        public ErrorDto Sif_OficinasMiembros_Agregar(int CodEmpresa, string oficina, string usuario, int apoyo, string usuarioRegistro, string accion)
        {
            return _db.Sif_OficinasMiembros_Agregar( CodEmpresa,  oficina,  usuario,  apoyo,  usuarioRegistro,  accion);
        }

        public ErrorDto<List<SifOficinasHistorial>> Sif_OficinasHistorial_Lista(int CodEmpresa,string filtro)
        {
            return _db.Sif_OficinasHistorial_Lista(CodEmpresa, filtro);
        }
        
        public ErrorDto<List<SifOficinasData>> Sif_Oficinas_Exportar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sif_Oficinas_Exportar(CodEmpresa, filtros);
        }
    }
}