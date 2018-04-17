using LNF.Billing;
using LNF.Repository;
using LNF.Repository.Billing;
using System;
using System.Data;

namespace sselFinOps.AppCode.BLL
{
    public static class FormulaBL
    {
        public static readonly DateTime July2010 = new DateTime(2010, 7, 1);
        public static readonly DateTime April2011 = new DateTime(2011, 4, 1);

        public static IBillingTypeManager BillingTypeManager => DA.Use<IBillingTypeManager>();

        [Obsolete("Use LNF.CommonTools.LineCostUtility.CalculateRoomLineCost instead.")]
        public static void ApplyRoomFormula(DataTable dtIn)
        {
            int billingTypeId;
            LabRoom room;

            foreach (DataRow dr in dtIn.Rows)
            {
                billingTypeId = dr.Field<int>("BillingTypeID");
                room = (LabRoom)dr.Field<int>("RoomID");

                //1. Find out all Monthly type users and apply to Clean room
                if (BillingTypeManager.IsMonthlyUserBillingType(billingTypeId))
                {
                    if (room == LabRoom.CleanRoom)
                        dr["LineCost"] = dr["MonthlyRoomCharge"];
                    else
                        dr["LineCost"] = dr.Field<decimal>("RoomCharge") + dr.Field<decimal>("EntryCharge");
                }
                //2. The growers are charged with room fee only when they reserve and activate a tool
                else if (BillingTypeManager.IsGrowerUserBillingType(billingTypeId))
                {
                    if (room == LabRoom.Organics)
                    {
                        //Organics bay must be charged for growers as well
                        dr["LineCost"] = dr["RoomCharge"];
                    }
                    else
                        dr["LineCost"] = (dr.Field<decimal>("AccountDays") * dr.Field<decimal>("RoomRate")) + dr.Field<decimal>("EntryCharge");
                }
                else if (billingTypeId == BillingType.Other)
                {
                    dr["LineCost"] = 0;
                }
                else
                {
                    //Per Use types
                    dr["LineCost"] = dr.Field<decimal>("RoomCharge") + dr.Field<decimal>("EntryCharge");
                }
            }
        }

        [Obsolete("Use LNF.CommonTools.LineCostUtility.CalculateToolLineCost instead.")]
        public static void ApplyToolFormula(DataTable dtIn, DateTime startPeriod, DateTime endPeriod)
        {
            int billingTypeId;
            LabRoom room;
            bool isStarted;

            foreach (DataRow dr in dtIn.Rows)
            {
                billingTypeId = dr.Field<int>("BillingTypeID");
                room = (LabRoom)dr.Field<int>("RoomID");
                isStarted = dr.Field<bool>("IsStarted");

                if (billingTypeId == BillingType.Int_Ga || billingTypeId == BillingType.Int_Si || billingTypeId == BillingType.ExtAc_Ga || billingTypeId == BillingType.ExtAc_Si)
                {
                    //Monthly User, charge maks maker for everyone
                    if (room == LabRoom.CleanRoom)
                    {
                        if (dr.Field<int>("ResourceID") == 56000)
                        {
                            if (isStarted)
                                dr["LineCost"] = dr.Field<double>("UsageFee") + dr.Field<double>("OverTimePenaltyFee") + (dr.Field<double>("ResourceRate") == 0 ? 0 : dr.Field<double>("ReservationFee2"));
                            else
                                dr["LineCost"] = dr.Field<double>("UncancelledPenaltyFee") + dr.Field<double>("ReservationFee2");
                        }
                        else
                        {
                            dr["LineCost"] = 0;
                        }
                    }
                    else //non clean room tools are always charged for usage fee
                    {
                        if (isStarted)
                            dr["LineCost"] = dr.Field<double>("UsageFee") + dr.Field<double>("OverTimePenaltyFee") + (dr.Field<double>("ResourceRate") == 0 ? 0 : dr.Field<double>("ReservationFee2"));
                        else
                            dr["LineCost"] = dr.Field<double>("UncancelledPenaltyFee") + dr.Field<double>("ReservationFee2");
                    }
                }
                else
                {
                    //Per Use types
                    if (startPeriod >= July2010)
                    {
                        //dr["LineCost"] = dr.Field<double>("UsageFee") + dr.Field<double>("OverTimePenaltyFee") + dr.Field<double>("UncancelledPenaltyFee") + dr.Field<double>("ReservationFee2");

                        //2011-05 New tool billing started on 2011-04
                        if (startPeriod >= April2011)
                        {
                            if (!dr.Field<bool>("IsCancelledBeforeAllowedTime"))
                                dr["LineCost"] = Convert.ToDecimal(dr["UsageFee20110401"]) + Convert.ToDecimal(dr["OverTimePenaltyFee"]) + Convert.ToDecimal(dr["BookingFee"]); //(dr.Field<double>("ResourceRate") == 0 ? 0 : dr.Field<double>("ReservationFee"))
                            else
                                dr["LineCost"] = dr["BookingFee"]; //Cancelled before two hours
                        }
                        else
                        {
                            if (isStarted)
                                dr["LineCost"] = dr.Field<double>("UsageFee") + dr.Field<double>("OverTimePenaltyFee") + (dr.Field<double>("ResourceRate") == 0 ? 0 : dr.Field<double>("ReservationFee2"));
                            else
                                dr["LineCost"] = dr.Field<double>("UncancelledPenaltyFee"); //+ dr.Field<double>("ReservationFee")
                        }

                        //2010-12-06 Sandrine doesn't want Remote to be shown
                        //2015-05-13 She must have changed her mind at some later date
                        bool showRemote = true;
                        if (billingTypeId == BillingType.Remote && !showRemote)
                            dr["LineCost"] = 0;
                    }
                    else
                    {
                        if (isStarted)
                            dr["LineCost"] = dr.Field<double>("UsageFeeOld") + dr.Field<double>("OverTimePenaltyFee") + (dr.Field<double>("ResourceRate") == 0 ? 0 : dr.Field<double>("ReservationFee2"));
                        else
                            dr["LineCost"] = dr.Field<double>("UncancelledPenaltyFee") + dr.Field<double>("ReservationFee2");
                    }

                    //if (isStarted)
                    //    dr["LineCost"] = dr.Field<double>("UsageFee") + dr.Field<double>("OverTimePenaltyFee") + dr.Field<double>("ReservationFee2");
                    //else
                    //    dr["LineCost"] = dr.Field<double>("UncancelledPenaltyFee") + dr.Field<double>("ReservationFee2");
                }

                //if resource rate is 0 , everything must be 0
                if (dr.Field<decimal>("ResourceRate") == 0)
                    dr["LineCost"] = 0;
            }
        }
    }
}
