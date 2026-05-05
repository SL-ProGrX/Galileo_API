using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogicTier.ProGrX.Cobros
{
    public class FrmCOAntiguedadTiposBL
    {
        private readonly FrmCOAntiguedadTiposDB _db;

        public FrmCOAntiguedadTiposBL(IConfiguration config)
        {
            _db = new FrmCOAntiguedadTiposDB(config);
        }

        public ErrorDto<FrmCOAntiguedadTiposListaResult> Co_AntiguedadTipos_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Co_AntiguedadTipos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<FrmCOAntiguedadTiposListaResult> Co_AntiguedadTipos_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;
            return _db.Co_AntiguedadTipos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Co_AntiguedadTipos_Guardar(int CodEmpresa, string usuario, FrmCOAntiguedadTipoData tipo)
        {
            return _db.Co_AntiguedadTipos_Guardar(CodEmpresa, usuario, tipo);
        }

        public ErrorDto Co_AntiguedadTipos_Eliminar(int CodEmpresa, string usuario, string cod_antiguedad)
        {
            return _db.Co_AntiguedadTipos_Eliminar(CodEmpresa, usuario, cod_antiguedad);
        }

        public ErrorDto<List<FrmCOAntiguedadGarantiaMitigadorData>> Co_AntiguedadTipos_Detalle_Obtener(int CodEmpresa, string cod_antiguedad, string usuario)
        {
            return _db.Co_AntiguedadTipos_Detalle_Obtener(CodEmpresa, cod_antiguedad, usuario);
        }

        public ErrorDto Co_AntiguedadTipos_Detalle_Guardar(int CodEmpresa, string usuario, FrmCOAntiguedadDetalleGuardarDto dto)
        {
            return _db.Co_AntiguedadTipos_Detalle_Guardar(CodEmpresa, usuario, dto);
        }
    }
}