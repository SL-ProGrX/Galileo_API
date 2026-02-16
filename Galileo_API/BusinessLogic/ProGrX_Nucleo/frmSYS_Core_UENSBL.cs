using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.SYS;

namespace Galileo.BusinessLogic
{
    public class FrmSysCoreUensBL(IConfiguration config)
    {
        private readonly FrmSysCoreUensDB _db = new FrmSysCoreUensDB(config);

        public ErrorDto<CoreUeNsDtoList> Core_UENS_Obtener(int CodCliente, string filtros)
        {
            return _db.Core_UENS_Obtener(CodCliente, filtros);
        }

        public ErrorDto Core_UENS_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            return _db.Core_UENS_Upsert(CodCliente, usuario, request);
        }

        public ErrorDto Core_SubUnidad_Upsert(int CodCliente, string usuario, string? unidad_anterior, CoreUeNsDto request)
        {
            return _db.Core_SubUnidad_Upsert(CodCliente, usuario, unidad_anterior, request);
        }

        public ErrorDto Core_SubCentroCosto_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            return _db.Core_SubCentroCosto_Upsert(CodCliente, usuario, request);
        }

        public ErrorDto Core_UENS_Delete(int CodCliente, string cod_unidad)
        {
            return _db.Core_UENS_Delete(CodCliente, cod_unidad);
        }

        public ErrorDto Core_SubUnidad_Delete(int CodCliente, string cod_unidad, string cntx_unidad)
        {
            return _db.Core_SubUnidad_Delete(CodCliente, cod_unidad, cntx_unidad);
        }

        public ErrorDto Core_SubCentroCosto_Delete(int CodCliente, string cod_unidad)
        {
            return _db.Core_SubCentroCosto_Delete(CodCliente, cod_unidad);
        }

        public ErrorDto<CoreUeNsDtoList> Core_UENSPrincipales_Obtener(int CodCliente, string filtros)
        {
            return _db.Core_UENSPrincipales_Obtener(CodCliente, filtros);
        }

        public ErrorDto<CoreUeNsDtoList> Core_SubUnidades_Obtener(int CodCliente, string cod_unidad)
        {
            return _db.Core_SubUnidades_Obtener(CodCliente, cod_unidad);
        }

        public ErrorDto<CoreUeNsDtoList> Core_SubCentroCosto_Obtener(int CodCliente, string cod_unidad, string sub_unidad)
        {
            return _db.Core_SubCentroCosto_Obtener(CodCliente, cod_unidad, sub_unidad);
        }

        public ErrorDto<List<CoreUsuariosDto>> Core_Miembros_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            return _db.Core_Miembros_Obtener(CodCliente, cod_unidad, filtro);
        }

        public ErrorDto Core_Miembros_Registro(int CodCliente, string cod_unidad, CoreUsuariosDto request)
        {
            return _db.Core_Miembros_Registro(CodCliente, cod_unidad, request);
        }

        public ErrorDto<List<CoreRolesDto>> Core_Roles_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            return _db.Core_Roles_Obtener(CodCliente, cod_unidad, filtro);
        }

        public ErrorDto Core_Roles_Registro(int CodCliente, string cod_unidad, CoreRolesDto request)
        {
            return _db.Core_Roles_Registro(CodCliente, cod_unidad, request);
        }

        public ErrorDto<List<UensListaDatos>> Core_UENLista_Obtener(int CodCliente, string? usuario)
        {
            return _db.Core_UENLista_Obtener(CodCliente, usuario);
        }

    }
}