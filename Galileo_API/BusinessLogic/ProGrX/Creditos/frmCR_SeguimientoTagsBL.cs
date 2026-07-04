using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRSeguimientoTagsBL
    {
        private readonly FrmCRSeguimientoTagsDB _db;

        public FrmCRSeguimientoTagsBL(IConfiguration config)
        {
            _db = new FrmCRSeguimientoTagsDB(config);
        }

        public ErrorDto<CrSeguimientoTagsUsuarioDto> CR_SeguimientoTags_Usuario_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return _db.CR_SeguimientoTags_Usuario_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoTags_Etiquetas_Dropdown_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return _db.CR_SeguimientoTags_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CrSeguimientoTagsOperacionDto> CR_SeguimientoTags_Operacion_Obtener(
            int CodEmpresa,
            long operacion)
        {
            return _db.CR_SeguimientoTags_Operacion_Obtener(CodEmpresa, operacion);
        }

        public ErrorDto<CrSeguimientoTagsLista> CR_SeguimientoTags_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            return _db.CR_SeguimientoTags_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrSeguimientoTagsLista> CR_SeguimientoTags_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return _db.CR_SeguimientoTags_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrSeguimientoTagsAplicarResult> CR_SeguimientoTags_Aplicar(
            int CodEmpresa,
            CrSeguimientoTagsAplicarRequest request)
        {
            return _db.CR_SeguimientoTags_Aplicar(CodEmpresa, request);
        }
    }
}