using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOGestionesMasivasBL
    {
        private readonly FrmCOGestionesMasivasDB _db;

        public FrmCOGestionesMasivasBL(IConfiguration config)
        {
            _db = new FrmCOGestionesMasivasDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CO_GestionesMasivas_Usuarios_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Gestiones_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CO_GestionesMasivas_Gestiones_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Causas_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CO_GestionesMasivas_Causas_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Arreglos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CO_GestionesMasivas_Arreglos_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<CoGestionesMasivasCargaResultDto> CO_GestionesMasivas_Cargar(int CodEmpresa,string usuarioSesion,CoGestionesMasivasCargaRequest request)
        {
            return _db.CO_GestionesMasivas_Cargar(CodEmpresa, usuarioSesion, request);
        }
        public ErrorDto CO_GestionesMasivas_Procesar(int CodEmpresa,string usuarioSesion,CoGestionesMasivasProcesarRequest request)
        {
            return _db.CO_GestionesMasivas_Procesar(CodEmpresa, usuarioSesion, request);
        }
    }
}