using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public sealed class FrmCrConsultaPlanillaAbonoDistBL
    {
        private readonly FrmCrConsultaPlanillaAbonoDistDB _db;

        public FrmCrConsultaPlanillaAbonoDistBL(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmCrConsultaPlanillaAbonoDistDB(config);
        }

        public ErrorDto<CrConsultaPlanillaAbonoDistInicialData>
            CR_ConsultaPlanillaAbonoDist_Inicializar(
                int codEmpresa,
                string cedula,
                string usuario)
        {
            return _db.CR_ConsultaPlanillaAbonoDist_Inicializar(
                codEmpresa,
                cedula,
                usuario);
        }

        public ErrorDto<CrConsultaPlanillaAbonoDistUltimoData>
            CR_ConsultaPlanillaAbonoDist_UltimoMonto(
                int codEmpresa,
                string cedula,
                int codInstitucion,
                int proceso)
        {
            return _db.CR_ConsultaPlanillaAbonoDist_UltimoMonto(
                codEmpresa,
                cedula,
                codInstitucion,
                proceso);
        }

        public ErrorDto<List<CrConsultaPlanillaAbonoDistDetalleData>>
            CR_ConsultaPlanillaAbonoDist_Consultar(
                int codEmpresa,
                CrConsultaPlanillaAbonoDistConsultaRequest? request)
        {
            return _db.CR_ConsultaPlanillaAbonoDist_Consultar(
                codEmpresa,
                request);
        }
    }
}