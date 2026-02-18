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

        /// <summary>
        /// Obtiene los datos de la operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene los movimientos de la operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCPlanPagosMovimientoData>> CxCPlanPagos_Movimientos_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select Mov.Linea,Mov.Fecha_Inicio,Mov.Fecha_Corte,Mov.Cargos,
                Mov.Int_Cor,Mov.Int_Mor,Mov.Principal,Mov.Saldo_Inicial,
                isnull(Mov.Saldo_Final, Mov.Saldo_Inicial - Mov.Principal) as saldo_final, 
                Mov.Dias,case Mov.Estado when 'A' then 'Activa' when 'P' then 'Pendiente' 
                when 'C' then 'Cancelada' when 'N' then 'Anulada' end as 'Estado', Mov.Dias_Mora,
                Mov.Registro_Fecha,Mov.Mov_Monto,Mov.Mov_Cargos,Mov.Mov_Int_Cor,Mov.Mov_Int_Mor,Mov.Mov_Principal,
                (Mov.Cod_Caja + '/' + Mov.Registro_Usuario) as caja_usuario, 
                Mov.Tipo_Documento,Mov.Num_Documento,Con.Descripcion as 'Concepto'
                from CxC_Cuentas_Mov Mov left join SIF_Conceptos Con on Mov.cod_concepto = Con.cod_Concepto
                where Mov.Operacion = @operacionId
                order by Mov.Linea;";

            return DbHelper.ExecuteListQuery<CxCPlanPagosMovimientoData>(
                _portalDb, codEmpresa, query, new { operacionId });
        }

        /// <summary>
        /// Obtiene el resumen de la operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene los cargos por movimiento de la operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <param name="estadoActivo"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCPlanPagosCargosMovData>> CxCPlanPagos_CargosPorMovimiento_Obtener(
            int codEmpresa, int operacionId, bool estadoActivo, int? linea)
        {
            string query;
            object param;

            if (estadoActivo)
            {
                query = @"select *, isnull(Monto - Saldo,0) as 'Abono',0 as 'Linea' 
                    from CxC_Cuentas_Cargos Car
                    where Car.OPERACION = @operacionId
                      and Car.SALDO > 0;";
                param = new { operacionId };
            }
            else
            {
                query = @"select Car.*, isnull(Mov.Monto,0) as 'Abono',isnull(Mov.Linea,0) as 'Linea' 
                    from CxC_Cuentas_Cargos Car
                    inner join CxC_Cuentas_Cargos_Mov Mov
                        on Car.ID_CARGO = Mov.ID_CARGO
                       and Car.OPERACION = Mov.OPERACION
                    where Car.OPERACION = @operacionId
                      and Mov.LINEA = @linea
                    order by Mov.LINEA;";
                param = new { operacionId, linea = linea ?? 0 };
            }

            return DbHelper.ExecuteListQuery<CxCPlanPagosCargosMovData>(
                _portalDb, codEmpresa, query, param);
        }
    }
}
