import React, { useState } from "react";
import "../styles/Filter.css";
import { currencyReverseMap } from "../utils/currency";

const Filter = ({ onSearch }) => {
  const [originIataCode, setOriginIataCode] = useState("");
  const [destinationIataCode, setDestinationIataCode] = useState("");
  const [departureDate, setDepartureDate] = useState("");
  const [returnDate, setReturnDate] = useState("");
  const [numberOfPassengers, setNumberOfPassengers] = useState(1);
  const [currency, setCurrency] = useState("USD");

  const handleSubmit = (e) => {
    e.preventDefault();

    // Pass filter data to the onSearch function
    onSearch({
      originIataCode,
      destinationIataCode,
      departureDate,
      returnDate,
      numberOfPassengers,
      currency,
    });
  };

  return (
    <div className="filter-container">
      <form onSubmit={handleSubmit}>
        <label>
          Origin IATA code:
          <input
            type="text"
            value={originIataCode}
            onChange={(e) => {
              const input = e.target.value.toUpperCase();
              if (input.length <= 3) {
                setOriginIataCode(input);
              }
            }}
            onInput={(e) => {
              e.target.value = e.target.value.toUpperCase().slice(0, 3);
            }}
            placeholder="E.g., JFK"
          />
        </label>
        <label>
          Destination IATA code:
          <input
            type="text"
            value={destinationIataCode}
            onChange={(e) => {
              const input = e.target.value.toUpperCase();
              if (input.length <= 3) {
                setDestinationIataCode(input);
              }
            }}
            onInput={(e) => {
              e.target.value = e.target.value.toUpperCase().slice(0, 3);
            }}
            placeholder="E.g., LAX"
          />
        </label>
        <label>
          Departure Date:
          <input
            type="date"
            value={departureDate}
            onChange={(e) => {
              const selectedDate = new Date(e.target.value);
              const now = new Date().setHours(0, 0, 0, 0); // Ensure comparison is on the same date level (no time)
              const returnDateValue = returnDate
                ? new Date(returnDate).setHours(0, 0, 0, 0)
                : null;

              // Ensure departure date is after today and before the return date
              if (
                selectedDate >= now &&
                (!returnDate || selectedDate < returnDateValue)
              ) {
                setDepartureDate(e.target.value);
              } else {
                alert(
                  "Departure date must be today or later and before the return date."
                );
              }
            }}
          />
        </label>
        <label>
          Return Date:
          <input
            type="date"
            value={returnDate}
            onChange={(e) => {
              const selectedDate = new Date(e.target.value);
              const departureDateValue = departureDate
                ? new Date(departureDate)
                : null;

              // If departureDate is set, ensure return date is after departure date
              if (departureDateValue && selectedDate < departureDateValue) {
                alert("Return date must be after the departure date.");
              } else {
                setReturnDate(e.target.value);
              }
            }}
          />
        </label>
        <label>
          Number of Passengers:
          <input
            type="number"
            min="1"
            max="9"
            value={numberOfPassengers}
            onChange={(e) => setNumberOfPassengers(Number(e.target.value))}
          />
        </label>
        <label>
          Currency:
          <select
            value={currency}
            onChange={(e) => setCurrency(e.target.value)}
            required
          >
            {Object.keys(currencyReverseMap).map((cur) => (
              <option key={cur} value={cur}>
                {cur}
              </option>
            ))}
          </select>
        </label>
        <button type="submit">Search Flights</button>
      </form>
    </div>
  );
};

export default Filter;
