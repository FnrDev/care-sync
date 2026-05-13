$(function () {
    var selectedSpec = null;
    var selectedSpecName = '';
    var selectedDoctor = null;
    var selectedDoctorName = '';

    // Step 1: Select specialization
    $('.spec-card').on('click', function () {
        $('.spec-card').removeClass('border-primary bg-primary bg-opacity-10');
        $(this).addClass('border-primary bg-primary bg-opacity-10');

        selectedSpec = $(this).data('spec-id');
        selectedSpecName = $(this).find('.card-title').text();
        $('#specializationId').val(selectedSpec);

        // Reset subsequent steps
        $('#step3, #step4').hide();
        $('#doctorProfileId').val('');
        $('#startTime').val('');

        // Load doctors
        $.get('/Booking/GetDoctors', { specId: selectedSpec }, function (doctors) {
            var html = '';
            if (doctors.length === 0) {
                html = '<div class="col-12"><p class="text-muted">No doctors available for this specialization.</p></div>';
            } else {
                doctors.forEach(function (doc) {
                    html += '<div class="col-md-4">' +
                        '<div class="card doctor-card border" data-doctor-id="' + doc.id + '" style="cursor:pointer;">' +
                        '<div class="card-body">' +
                        '<h6 class="card-title">' + doc.name + '</h6>' +
                        '<p class="text-muted small mb-1"><i class="bi bi-clock me-1"></i>' + doc.consultationDurationMin + ' min consultation</p>' +
                        (doc.bio ? '<p class="small mb-0">' + doc.bio + '</p>' : '') +
                        '</div></div></div>';
                });
            }
            $('#doctorList').html(html);
            $('#step2').show();

            // Bind doctor selection
            $('.doctor-card').on('click', function () {
                $('.doctor-card').removeClass('border-primary bg-primary bg-opacity-10');
                $(this).addClass('border-primary bg-primary bg-opacity-10');

                selectedDoctor = $(this).data('doctor-id');
                selectedDoctorName = $(this).find('.card-title').text();
                $('#doctorProfileId').val(selectedDoctor);

                // Reset time
                $('#startTime').val('');
                $('#step4').hide();
                $('#appointmentDate').val('');
                $('#slotContainer').html('<p class="text-muted">Select a date to see available slots.</p>');
                $('#step3').show();
            });
        });
    });

    // Step 3: Date change -> load slots
    $('#appointmentDate').on('change', function () {
        var date = $(this).val();
        if (!date || !selectedDoctor) return;

        $('#slotContainer').html('<p class="text-muted">Loading slots...</p>');
        $('#step4').hide();

        $.get('/Booking/GetAvailableSlots', { doctorId: selectedDoctor, date: date }, function (slots) {
            var html = '';
            if (slots.length === 0) {
                html = '<p class="text-warning">No available slots for this date. Please try another date.</p>';
            } else {
                slots.forEach(function (slot) {
                    html += '<button type="button" class="btn btn-outline-primary slot-btn" data-start="' +
                        slot.startTime + '" data-end="' + slot.endTime + '">' +
                        slot.startTime + ' - ' + slot.endTime + '</button>';
                });
            }
            $('#slotContainer').html(html);

            // Bind slot selection
            $('.slot-btn').on('click', function () {
                $('.slot-btn').removeClass('btn-primary').addClass('btn-outline-primary');
                $(this).removeClass('btn-outline-primary').addClass('btn-primary');

                var start = $(this).data('start');
                var end = $(this).data('end');
                $('#startTime').val(start);

                // Show confirmation
                $('#confirmSpec').text(selectedSpecName);
                $('#confirmDoctor').text(selectedDoctorName);
                $('#confirmDate').text(new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }));
                $('#confirmTime').text(start + ' - ' + end);
                $('#step4').show();
            });
        });
    });
});
