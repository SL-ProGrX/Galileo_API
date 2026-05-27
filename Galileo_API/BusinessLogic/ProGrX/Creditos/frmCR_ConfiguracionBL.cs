using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRConfiguracionBL
    {
        private readonly FrmCRConfiguracionDB _db;

        public FrmCRConfiguracionBL(IConfiguration config)
        {
            _db = new FrmCRConfiguracionDB(config);
        }
        public ErrorDto<List<CrConfiguracionGeneralDto>> CR_Configuracion_Generales_Lista_Obtener(int CodEmpresa)
        {
            return _db.CR_Configuracion_Generales_Lista_Obtener(CodEmpresa);
        }
        public ErrorDto<List<CrConfiguracionGeneralDto>> CR_Configuracion_Generales_Lista_Export(int CodEmpresa)
        {
            return _db.CR_Configuracion_Generales_Lista_Export(CodEmpresa);
        }
        public ErrorDto CR_Configuracion_Generales_Guardar(int CodEmpresa,CrConfiguracionGeneralGuardarDto request,string usuario)
        {
            return _db.CR_Configuracion_Generales_Guardar(CodEmpresa, request, usuario);
        }
        public ErrorDto<CrConfiguracionOperativosDto> CR_Configuracion_Operativos_Obtener(int CodEmpresa)
        {
            return _db.CR_Configuracion_Operativos_Obtener(CodEmpresa);
        }
        public ErrorDto CR_Configuracion_Operativos_Guardar(int CodEmpresa,CrConfiguracionOperativosGuardarDto request,string usuario)
        {
            return _db.CR_Configuracion_Operativos_Guardar(CodEmpresa, request, usuario);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Configuracion_Bancos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CR_Configuracion_Bancos_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Configuracion_TiposDocumento_Dropdown_Obtener()
        {
            return _db.CR_Configuracion_TiposDocumento_Dropdown_Obtener();
        }
        public ErrorDto CR_Configuracion_FechaCorte_Guardar(int CodEmpresa,CrConfiguracionFechaCorteGuardarDto request,string usuario)
        {
            return _db.CR_Configuracion_FechaCorte_Guardar(CodEmpresa, request, usuario);
        }
        public ErrorDto CR_Configuracion_TBP_Guardar(int CodEmpresa,CrConfiguracionTbpGuardarDto request,string usuario)
        {
            return _db.CR_Configuracion_TBP_Guardar(CodEmpresa, request, usuario);
        }
    }
}