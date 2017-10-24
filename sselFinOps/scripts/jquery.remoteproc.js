(function ($) {
    $.fn.remoteproc = function () {
        return this.each(function () {
            var $this = $(this);

            var getApiUrl = function () {
                return '/webapi/'
            }

            var getYear = function () {
                return parseInt($('.year-select', $this).val());
            }

            var getMonth = function () {
                return parseInt($('.month-select', $this).val());
            }

            var getPeriod = function () {
                return moment(new Date(getYear(), getMonth() - 1, 1));
            }

            var getRegularExceptionUrl = function () {
                return getApiUrl() + 'billing/report/regular-exception?period=' + getPeriod().format('YYYY-MM-DD');
            }

            var getRemoteProcUrl = function () {
                return getApiUrl() + 'scheduler/reservation?sd=' + getPeriod().format('YYYY-MM-DD') + "&ed=" + getPeriod().add(1, 'M').format('YYYY-MM-DD') + "&activityId=22";
            }

            var getStaffUrl = function () {
                return getApiUrl() + 'data/client/active?sd=' + getPeriod().format('YYYY-MM-DD') + '&ed=' + getPeriod().add(1, 'M').format('YYYY-MM-DD') + '&privs=2';
            }

            var getRemoteUserUrl = function () {
                return getApiUrl() + 'data/client/active?sd=' + getPeriod().format('YYYY-MM-DD') + '&ed=' + getPeriod().add(1, 'M').format('YYYY-MM-DD') + '&privs=128';
            }

            var getRemoteAccountUrl = function (clientId) {
                return getApiUrl() + 'data/client/' + clientId + '/accounts/active?sd=' + getPeriod().format('YYYY-MM-DD') + '&ed=' + getPeriod().add(1, 'M').format('YYYY-MM-DD');
            }

            var fillStaffSelect = function () {
                var select = $('.staff.user-select', $this);

                select.hide();
                $(".staff-loading", $this).show();

                $.ajax({
                    'url': getStaffUrl()
                }).done(function (data) {
                    select.html($.map(data, function (val, i) {
                        return $('<option/>', { "value": val.ClientID }).html(val.LName + ', ' + val.FName);
                    }));

                    select.show();
                    $(".staff-loading", $this).hide();
                });
            }

            var fillRemoteUserSelect = function (callback) {
                var select = $(".remote.user-select", $this);

                select.hide().prop('disabled', false);
                $(".remote-user-loading", $this).show();

                $.ajax({
                    'url': getRemoteUserUrl()
                }).done(function (data) {

                    if (data.length == 0) {
                        select.html($("<option/>").val(0).html("No active remote users this period")).prop('disabled', true);
                        return;
                    }

                    select.html($.map(data, function (val, i) {
                        return $('<option/>', { "value": val.ClientID }).html(val.LName + ', ' + val.FName);
                    }));

                    select.show();
                    $(".remote-user-loading", $this).hide();

                    if (typeof callback == 'function')
                        callback();
                }).fail(function (err) {
                    alert(err);
                });
            }

            var fillRemoteAccountSelect = function () {
                var select = $(".remote.account-select", $this);

                select.hide().prop('disabled', false);
                $(".remote-account-loading", $this).show();

                var clientId = $(".remote.user-select", $this).val();

                $.ajax({
                    'url': getRemoteAccountUrl(clientId)
                }).done(function (data) {

                    if (data.length == 0) {
                        select.html($("<option/>").val(0).html("No active accounts this period")).prop('disabled', true);
                        return;
                    }

                    select.html($.map(data, function (val, i) {
                        return $('<option/>', { "value": val.AccountID }).html(val.AccountName);
                    }));
                }).always(function () {
                    select.show();
                    $(".remote-account-loading", $this).hide();
                });
            }

            // ------------------ Regular Exceptions ------------------------
            var regExcepTable = $('.regular-exception-table', $this).dataTable({
                'processing': false,
                'ajax': {
                    'url': getRegularExceptionUrl(),
                    'dataSrc': function (json) {
                        return json;
                    }
                },
                'columns': [
                    { 'title': 'Period', 'data': 'Period' },
                    {
                        'title': 'Client', 'data': function (row, type, set, meta) {
                            return row.LName + ', ' + row.FName;
                        }
                    },
                    {
                        'title': 'Invitee', 'data': function (row, type, set, meta) {
                            if (row.InviteeClientID > 0) {
                                return row.InviteeLName + ", " + row.InviteeFName;
                            } else {
                                return '';
                            }
                        }
                    },
                    {
                        'title': 'Reservation ID', 'width': '100px', 'data': function (row, type, set, meta) {
                            if (row.ReservationID == 0)
                                return '';
                            else
                                return row.ReservationID;
                        }
                    },
                    { 'title': 'Resource/Room', 'data': 'ResourceName' },
                    { 'title': 'AccountName', 'data': 'AccountName' }
                ],
                'order': [[0, 'asc']]
            });

            // ------------------ Remote Processing Reservations ------------------------
            var remProcTable = $('.remote-processing-table', $this).dataTable({
                'processing': false,
                'ajax': {
                    'url': getRemoteProcUrl(),
                    'dataSrc': function (json) {
                        return json;
                    }
                },
                'columns': [
                    { 'title': 'Reservation ID', 'width': '70px', 'data': 'ReservationID' },
                    { 'title': 'Resource', 'data': 'ResourceName' },
                    {
                        'title': 'Client', 'width': '120px', 'data': function (row, type, set, meta) {
                            return row.LName + ', ' + row.FName;
                        }
                    },
                    {
                        'title': 'Remote<br>Client', 'width': '120px', 'data': function (row, type, set, meta) {
                            if (row.Invitees.length) {
                                return row.Invitees[0].LName + ", " + row.Invitees[0].FName;
                            } else {
                                return 'n/a';
                            }
                        }
                    },
                    { 'title': 'Account', 'data': 'AccountName' },
                    {
                        'title': 'Begin<br>Time', 'data': function (row, type, set, meta) {
                            return moment(row.BeginDateTime).format("M/D/YYYY[<br>]h:mm A");
                        }
                    },
                    {
                        'title': 'End<br>Time', 'data': function (row, type, set, meta) {
                            return moment(row.EndDateTime).format("M/D/YYYY[<br>]h:mm A");
                        }
                    },
                    { 'title': 'Active', 'className': 'align-center', 'width': '65px', 'data': 'IsActive' },
                    { 'title': 'Started', 'className': 'align-center', 'width': '65px', 'data': 'IsStarted' },
                    { 'title': 'CM', 'className': 'align-right', 'width': '50px', 'data': 'ChargeMultiplier' }
                ],
                'order': [[5, 'asc']]
            });

            fillStaffSelect();
            fillRemoteUserSelect(function () {
                fillRemoteAccountSelect();
            });

            $this.on('refresh', function (e) {
                regExcepTable.api().ajax.reload();
                remProcTable.api().ajax.reload();
            }).on('change', '.remote.user-select', function (e) {
                fillRemoteAccountSelect();
            });

            //var refreshTables = function () {
            //    
            //}
        });
    }
}(jQuery));
