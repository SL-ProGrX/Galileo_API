using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCPlanPagosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCxCPlanPagosDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCxCPlanPagosDb(PortalDB portalDB)
        {
            _portalDb = portalDB;
        }

        public ErrorDto<CxCPlanPagosOperacionData> CxCPlanPagos_Operacion_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select R.Operacion,S.cedula,S.nombre,R.cod_Concepto,C.descripcion,R.Monto, 
                R.Saldo,R.cuota,R.Tasa_Corriente as 'TasaO',R.Tipo_Plazo,  R.Dias_Plazo as 'Plazo', 
                Ofi.descripcion as 'OficinaX',Con.Descripcion as 'Contrato',Pag.Nombre as 'Pagador', 
                R.Num_Documento,R.Fecha_Pago 
                from CxC_Personas S inner join CxC_Cuentas R on S.cedula = R.cedula 
                inner join CxC_Conceptos C on R.cod_Concepto = C.cod_Concepto 
                left join CxC_Contratos Con on R.cod_Contrato = Con.cod_contrato 
                left join CxC_Personas Pag on R.cedula_pagador = Pag.cedula 
                left join SIF_Oficinas Ofi on R.cod_oficina = Ofi.cod_oficina 
                where R.Operacion = @operacionId;";

            var result = DbHelper.ExecuteSingleQuery<CxCPlanPagosOperacionData>(
                _portalDb, codEmpresa, query, new CxCPlanPagosOperacionData(), new { operacionId });

            if (result.Result == null)
            {
                result.Result = new CxCPlanPagosOperacionData();
            }
            return result!;
        }

        public ErrorDto<List<CxCPlanPagosMovimientoData>> CxCPlanPagos_Movimientos_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select Mov.Linea,Mov.Fecha_Inicio,Mov.Fecha_Corte,Mov.Cargos
                ,Mov.Int_Cor,Mov.Int_Mor,Mov.Principal,Mov.Saldo_Inicial,
                isnull(Mov.Saldo_Final, Mov.Saldo_Inicial - Mov.Principal) as saldo_final, 
                Mov.Dias,case Mov.Estado when 'A' then 'Activa' when 'P' then 'Pendiente' 
                when 'C' then 'Cancelada' when 'N' then 'Anulada' end as 'Estado', Mov.Dias_Mora
                ,Mov.Registro_Fecha,Mov.Mov_Monto,Mov.Mov_Cargos,Mov.Mov_Int_Cor,Mov.Mov_Int_Mor,Mov.Mov_Principal,
                (Mov.Cod_Caja + '/' + Mov.Registro_Usuario) as caja_usuario, 
                ,Mov.Tipo_Documento,Mov.Num_Documento,Con.Descripcion as 'Concepto'
                from CxC_Cuentas_Mov Mov left join SIF_Conceptos Con on Mov.cod_concepto = Con.cod_Concepto
                where Mov.Operacion = @operacionId
                order by Mov.Linea;";

            return DbHelper.ExecuteListQuery<CxCPlanPagosMovimientoData>(
                _portalDb, codEmpresa, query, new { operacionId });
        }

        public ErrorDto<CxCPlanPagosOperacionResumenData> CxCPlanPagos_ResumenOperacion_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select max(Linea) as Lineas, sum(Int_Cor + Int_Mor) as Intereses, Sum(Cargos) as Cargos
                , sum(Dias) as Dias, min(Fecha_Corte) as Inicio, max(Fecha_Corte) as Corte, Sum(Dias_Mora) as MoraDias
                from CxC_Cuentas_Mov
                where isnull(Linea_Madre,0) = 0 and Operacion = @operacionId;";

            var result = DbHelper.ExecuteSingleQuery<CxCPlanPagosOperacionResumenData>(
                _portalDb, codEmpresa, query, new CxCPlanPagosOperacionResumenData(), new { operacionId });

            if (result.Result == null)
            {
                result.Result = new CxCPlanPagosOperacionResumenData();
            }
            return result!;
        }
    }
}
