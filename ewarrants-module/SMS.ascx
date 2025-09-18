<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SMS.ascx.cs" Inherits="tjc.Modules.eWarrants.SMS" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<asp:Literal ID="ltMesssage" runat="server" />
<div id="phoneNumbersContainer">
    <div class="form-group mb-3 phone-number-group">
        <label for="txtPhoneNumber" class="control-label">Phone Number (format: +1234567890):</label>
        <div class="input-group">
            <input type="text" id="txtPhoneNumber" class="form-control phone-number-input" style="width: 300px" placeholder="Enter phone number in international format" />
            <button type="button" class="btn btn-outline-danger remove-phone" style="display: none;"><em class="fa fa-trash"></em></button>
        </div>
    </div>
</div>
<asp:HiddenField ID="hdPhoneNumbers" ClientIDMode="Static" runat="server" />
<asp:Literal ID="ltPhoneError" runat="server" />
<div id="dvPhoneError"></div>
<button type="button" id="btnAddPhone" class="btn btn-success mb-3">Add Another Phone Number</button>
<div class="form-check mb-3">
    <asp:CheckBox ID="chkSmsConsent" ClientIDMode="Static" runat="server" CssClass="form-check-input" />
    <asp:Label ID="lblSmsConsent" runat="server" AssociatedControlID="chkSmsConsent" CssClass="form-check-label">
                I agree to receive SMS notifications (e.g., warrant updates, alerts). Msg & data rates may apply. Reply STOP to unsubscribe.
    </asp:Label>
</div>
<hr />
<asp:Button ID="btnOptOut" runat="server" Text="Opt Out" CssClass="btn btn-default" OnClick="btnOptOut_Click" CausesValidation="false" />
<asp:Button ID="btnSaveOptIn" ClientIDMode="Static" runat="server" Text="Opt In Save Numbers" CssClass="btn btn-primary" OnClick="btnSaveOptIn_Click" />
<asp:HyperLink ID="lnkCancel" Text="Cancel" CssClass="btn btn-danger pull-right" runat="server" />
<dnn:DnnJsInclude runat="server" FilePath="/Resources/libraries/libphonenumber/libphonenumber-js.min.js" />

<script type="text/javascript">
    function pageLoad(sender, args) {
        $(document).ready(function () {
            $('[data-toggle="tooltip"]').tooltip();
            $(".toolbar").append($("#cmdRefresh"));
            var hasOpted = '<%= HasUserOptedForSms() ? "true" : "false" %>';
            $('#dvPhoneError').html('');

            // Populate existing phone numbers
            var phoneNumbers = '<%= GetPhoneNumbersJson() %>';
            if (phoneNumbers) {
                var numbers = JSON.parse(phoneNumbers);
                if (numbers.length > 0) {
                    $('#phoneNumbersContainer').empty();
                    numbers.forEach(function (number) {
                        addPhoneNumberField(number);
                    });
                }
            }

            // Handle adding new phone number fields
            $('#btnAddPhone').on('click', function () {
                addPhoneNumberField('');
            });

            // Handle removing phone number fields
            $(document).on('click', '.remove-phone', function () {
                if ($('.phone-number-group').length > 1) {
                    $(this).closest('.phone-number-group').remove();
                }
                updateRemoveButtons();
            });

            // Update remove button visibility
            function updateRemoveButtons() {
                $('.remove-phone').each(function () {
                    $(this).toggle($('.phone-number-group').length > 1);
                });
            }

            // Add phone number field
            function addPhoneNumberField(number) {
                var newField = `
                    <div class="form-group mb-3 phone-number-group">
                        <label class="control-label">Phone Number (e.g., +1234567890):</label>
                        <div class="input-group">
                            <input type="text" class="form-control phone-number-input" style="width:300px" value="${number}" placeholder="Enter phone number in international format" />
                            <button type="button" class="btn btn-outline-danger remove-phone"><em class="fa fa-trash"></em></button>
                        </div>
                    </div>`;
                $('#phoneNumbersContainer').append(newField);
                updateRemoveButtons();
            }

            // Client-side validation and phone number reformatting on save click
            $('#btnSaveOptIn').on('click', function (e) {
                // Validate chkSmsConsent
                var consentChecked = $('#chkSmsConsent').is(':checked');
                if (!consentChecked) {
                    $('#dvPhoneError').html('<ul class="error-list"><li>You must agree to receive SMS notifications to opt in.</li></ul>');
                    e.preventDefault();
                    return false;
                }

                var phoneInputs = $('.phone-number-input');
                var numbers = [];
                var hasError = false;

                phoneInputs.each(function () {
                    if (hasError) return false; // Exit loop if an error has occurred

                    var val = $.trim($(this).val());
                    if (val !== '') {
                        try {
                            // Remove non-numeric characters
                            var cleanedNumber = val.replace(/\D/g, '');
                            // Prepend +1 if it doesn't start with +1
                            if (!cleanedNumber.startsWith('+1')) {
                                cleanedNumber = '+1' + cleanedNumber;
                            }
                            // Use libphonenumber-js to parse and reformat to E.164
                            var phoneNumber = libphonenumber.parsePhoneNumber(cleanedNumber);
                            if (phoneNumber.isValid()) {
                                numbers.push(phoneNumber.format('E.164'));
                            } else {
                                $('#dvPhoneError').html('<ul class="error-list"><li>Invalid phone number: ' + val + '. Please ensure it is a valid number.</li></ul>');
                                hasError = true;
                                e.preventDefault();
                                return false;
                            }
                        } catch (error) {
                            $('#dvPhoneError').html('<ul class="error-list"><li>Unable to parse phone number: ' + val + '. Please ensure it is a valid number.</li></ul>');
                            hasError = true;
                            e.preventDefault();
                            return false;
                        }
                    }
                });

                if (hasError) {
                    return false; // Prevent further processing if an error occurred
                }

                if (numbers.length === 0) {
                    $('#dvPhoneError').html('<ul class="error-list"><li>At least one phone number is required to opt in.</li></ul>');
                    e.preventDefault();
                    return false;
                }

                $('#hdPhoneNumbers').val(JSON.stringify(numbers));
            });
        });
    }
</script>