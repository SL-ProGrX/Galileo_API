using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.TES;

namespace Galileo_API.BusinessLogic.TES
{
    public class FrmTesConsultaDesembolsosBL
    {
        private readonly FrmTesConsultaDesembolsosDB _db;

        public FrmTesConsultaDesembolsosBL(IConfiguration config)
        {
            _db = new FrmTesConsultaDesembolsosDB(config);
        }

        public ErrorDto VerificarAutorizacion(int codEmpresa, string usuario)
        {
            return _db.VerificarAutorizacion(codEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int codEmpresa)
        {
            return _db.TES_Bancos_Grupos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Cuentas_Obtener(int codEmpresa, string usuario, string? codGrupo = null)
        {
            return _db.TES_Bancos_Cuentas_Obtener(codEmpresa, usuario, codGrupo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int codEmpresa)
        {
            return _db.TES_Conceptos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Documentos_Obtener(int codEmpresa)
        {
            return _db.TES_Tipos_Documentos_Obtener(codEmpresa);
        }
        
        public ErrorDto<DesembolsosLista> Desembolsos_Buscar(int codEmpresa,int CodConta,FiltrosBusqueda filtros)
        {
            return _db.Desembolsos_Buscar(codEmpresa,CodConta,filtros);
        }

        public ErrorDto<List<Desembolsos>> Desembolsos_Exportar(int codEmpresa, int CodConta, FiltrosBusqueda filtros)
        {
            return _db.Desembolsos_Exportar(codEmpresa, CodConta, filtros);
        }


    }

}