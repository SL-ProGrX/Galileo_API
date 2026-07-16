using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Galileo.Models;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaOperacionRenumeraBL
    {
        private readonly FrmCrApaOperacionRenumeraDB _db;

        public FrmCrApaOperacionRenumeraBL(IConfiguration config)
        {
            _db = new FrmCrApaOperacionRenumeraDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_OperacionRenumera_Acreedores_Obtener(
            int codEmpresa)
            => _db.CR_APA_OperacionRenumera_Acreedores_Obtener(codEmpresa);

        public ErrorDto<DropDownListaGenericaModel?> CR_APA_OperacionRenumera_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
            => _db.CR_APA_OperacionRenumera_Acreedor_Obtener(codEmpresa, cod_acreedor);

        public ErrorDto<FrmCrApaOperacionRenumeraOperacionDto?> CR_APA_OperacionRenumera_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
            => _db.CR_APA_OperacionRenumera_Operacion_Obtener(codEmpresa, cod_acreedor, operacion);

        public ErrorDto<List<FrmCrApaOperacionRenumeraOperacionDto>> CR_APA_OperacionRenumera_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor)
            => _db.CR_APA_OperacionRenumera_Operaciones_Obtener(codEmpresa, cod_acreedor);

        public ErrorDto<FrmCrApaOperacionRenumeraResultadoDto> CR_APA_OperacionRenumera_Aplicar(
            int codEmpresa,
            FrmCrApaOperacionRenumeraAplicarRequest request)
            => _db.CR_APA_OperacionRenumera_Aplicar(codEmpresa, request);
    }
}