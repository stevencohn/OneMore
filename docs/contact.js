$(document).ready(function ($) {

	// https://jqueryvalidation.org/
	const day = new Date().getDate();
	$('#contactForm').validate({
		rules: {
			honey: {
				required: true,
				digits: true,
				range: [day, day]
			}
		}
	});

	$('#contactForm').on('change keyup paste', function () {
		$('#submit').prop('disabled', !$('#contactForm').valid());
	});

	$('#contactForm').on('submit', function (e) {
		e.preventDefault();

		// build JSON; no need to be fancy here since this is for one specific form
		var data = {};
		$("#contactForm").serializeArray().map(function (x) { data[x.name] = x.value; });

		//console.log('data',data);
		//console.log('url', "https://" + window.location.host + "/rest/contact/");

		$.ajax({
			url: "https://formspree.io/f/mpzoybae",
			//url: "/prod/rest/contact/",
			//url: "https://" + window.location.host + "/rest/contact/",
			type: "POST",
			//contentType: "application/json; charset=utf-8",
			//dataType: "json",
			data: data,
			cache: false,
			success: function (response) { console.log("sent"); },
			error: function (msg) { console.log("error", msg); }
		});

		$("#contactName").val("");
		$("#contactEmail").val("");
		$("#contactMessage").val("");
		$("#honey").val("");
		$("#honeyp").val("");
		$("#submit").prop("disabled", true);

		$("#ccc").html("Thank you!");
	});
});
