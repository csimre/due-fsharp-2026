const setFieldValue = (form, name, value) => {
	const field = form?.querySelector(`[name="${name}"]`);
	if (!field) {
		return;
	}

	if (field.type === "checkbox") {
		field.checked = value === true || value === "true";
		return;
	}

	field.value = value ?? "";
};

const registerRowFill = (rowSelector, formId, fieldMap) => {
	const form = document.getElementById(formId);
	if (!form) {
		return;
	}

	document.querySelectorAll(rowSelector).forEach((row) => {
		row.addEventListener("click", (event) => {
			if (event.target.closest("form, button, a, input, select, textarea, label")) {
				return;
			}

			Object.entries(fieldMap).forEach(([fieldName, dataKey]) => {
				setFieldValue(form, fieldName, row.dataset[dataKey]);
			});

			form.scrollIntoView({ behavior: "smooth", block: "center" });
		});
	});
};

document.addEventListener("DOMContentLoaded", () => {
	registerRowFill(".js-fill-user", "update-user-form", {
		id: "id",
		name: "name",
		email: "email",
		phone: "phone",
		membershipType: "membershipType",
		active: "active"
	});

	registerRowFill(".js-fill-opening-hours", "update-opening-hours-form", {
		id: "id",
		dayOfWeek: "dayOfWeek",
		openTime: "openTime",
		closeTime: "closeTime"
	});

	registerRowFill(".js-fill-capacity", "update-capacity-form", {
		id: "id",
		date: "date",
		maxCapacity: "maxCapacity",
		currentCount: "currentCount"
	});

	registerRowFill(".js-fill-booking", "update-booking-form", {
		id: "id",
		userId: "userId",
		bookingDate: "bookingDate",
		bookingTime: "bookingTime",
		status: "status"
	});
});
