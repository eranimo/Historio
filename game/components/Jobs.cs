// component on entities that provides jobs
// sibling of Location
public class JobProvider {
	public List<Job> jobs;
}

public class JobLaborRequirement {
	public PopProfession profession;
	public int laborPower;
	public float currentLaborPower;
}

public class Job {
	public JobStatus status = JobStatus.Inactive;
	public bool isRepeating = false;
	public List<JobLaborRequirement> requirements;
}

public enum JobStatus {
	Inactive,
	Active,
	Finished,
}

public enum JobType {
	Construction,
	Logging,
	Farming,
	Production,
}