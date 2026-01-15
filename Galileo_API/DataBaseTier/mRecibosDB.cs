using Dapper;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier
{
    public class MRecibos
    {
        private readonly PortalDB _portalDB;

        public MRecibos(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        public long fxDocumentoConsecutivo(int codEmpresa, string vTipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                string strCampo = "", strUpdate = "";
                var query = "SELECT SysDocVersion FROM SIF_EMPRESA WHERE PORTAL_ID = @codEmpresa";
                if(conn.Query<int>(query, codEmpresa).FirstOrDefault() == 1)
                {
                    switch (vTipo)
                    {
                        case "RE":
                            strCampo = "select CS_RECIBO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_RECIBO = CS_RECIBO + 1";
                            break;
                        case "DP":
                            strCampo = "select CS_DEPOSITO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_DEPOSITO = CS_DEPOSITO + 1";
                            break;
                        case "ND":
                            strCampo = "select CS_NOTA_DEBITO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_NOTA_DEBITO = CS_NOTA_DEBITO + 1";
                            break;
                        case "NC":
                            strCampo = "select CS_NOTA_CREDITO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_NOTA_CREDITO = CS_NOTA_CREDITO + 1";
                            break;
                    }

                    long consecutivo = conn.Query<long>(strCampo).FirstOrDefault();

                    if (consecutivo == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        conn.Execute(strUpdate);
                    }

                        return consecutivo;
                }
                else
                {
                    strCampo = "exec spSIFDocsConsecutivo @vTipo";
                }

                return conn.Query<long>(strCampo, new {vTipo}).FirstOrDefault();

            }
            catch (Exception)
            {
                return 0;
            }

        }
    }
}
