using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTipoEsBL
    {
        private readonly FrmInvTipoEsDB _db;

        public FrmInvTipoEsBL(IConfiguration config)
        {
            _db = new FrmInvTipoEsDB(config);
        }

        public ErrorDto<TipoESList> TipoES_Obtener(int CodEmpresa, string filtros)
        {
            return _db.TipoES_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<TipoEsDto>> TipoES_Buscar(int CodEmpresa, string Tipo)
        {
            return _db.TipoES_Buscar(CodEmpresa, Tipo);
        }

        public ErrorDto TipoES_Insertar(int CodEmpresa, TipoEsDto request)
        {
            return _db.TipoES_Insertar(CodEmpresa, request);
        }

        public ErrorDto TipoES_Actualizar(int CodEmpresa, TipoEsDto request)
        {
            return _db.TipoES_Actualizar(CodEmpresa, request);
        }

        public ErrorDto TipoES_Eliminar(int CodEmpresa, string tipoES)
        {
            return _db.TipoES_Eliminar(CodEmpresa, tipoES);
        }
    }
}